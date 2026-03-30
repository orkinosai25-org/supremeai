#!/usr/bin/env bash
# SupremeAI – Azure AI infrastructure setup script
# Usage: ./setup.sh [options]
#
# Options:
#   --resource-group  <name>   Resource group name           (default: supremeai-rg)
#   --location        <loc>    Azure region                  (default: eastus)
#   --base-name       <name>   Base name for all resources   (default: supremeai)
#   --subscription    <id>     Azure subscription ID         (uses current if omitted)
#   --principal-id    <id>     Object ID granted secrets access in Key Vault (optional)
#   --help                     Show this help message

set -euo pipefail

# ── Defaults ─────────────────────────────────────────────────────────────────
RESOURCE_GROUP="supremeai-rg"
LOCATION="eastus"
BASE_NAME="supremeai"
SUBSCRIPTION_ID=""
PRINCIPAL_ID=""
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ── Colour helpers ────────────────────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
info()    { echo -e "${GREEN}[INFO]${NC} $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC} $*"; }
error()   { echo -e "${RED}[ERROR]${NC} $*" >&2; }

# ── Parse arguments ───────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --resource-group) RESOURCE_GROUP="$2"; shift 2 ;;
    --location)       LOCATION="$2";       shift 2 ;;
    --base-name)      BASE_NAME="$2";      shift 2 ;;
    --subscription)   SUBSCRIPTION_ID="$2";shift 2 ;;
    --principal-id)   PRINCIPAL_ID="$2";   shift 2 ;;
    --help)
      sed -n '/^# Usage/,/^[^#]/{ /^[^#]/d; s/^# \?//; p }' "$0"
      exit 0 ;;
    *) error "Unknown argument: $1"; exit 1 ;;
  esac
done

# ── Prerequisites check ───────────────────────────────────────────────────────
check_prerequisites() {
  info "Checking prerequisites…"
  if ! command -v az &>/dev/null; then
    error "Azure CLI (az) is not installed. Install from https://aka.ms/installazurecliwindows or via brew/apt."
    exit 1
  fi
  AZ_VERSION=$(az version --query '"azure-cli"' -o tsv 2>/dev/null || echo "0")
  info "Azure CLI version: $AZ_VERSION"
}

# ── Azure login check ─────────────────────────────────────────────────────────
ensure_logged_in() {
  info "Checking Azure login status…"
  if ! az account show &>/dev/null; then
    warn "Not logged in. Initiating login…"
    az login
  fi
  if [[ -n "$SUBSCRIPTION_ID" ]]; then
    az account set --subscription "$SUBSCRIPTION_ID"
    info "Using subscription: $SUBSCRIPTION_ID"
  else
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    info "Using current subscription: $SUBSCRIPTION_ID"
  fi
}

# ── Register required resource providers ─────────────────────────────────────
register_providers() {
  info "Registering required Azure resource providers…"
  local providers=(
    "Microsoft.CognitiveServices"
    "Microsoft.MachineLearningServices"
    "Microsoft.KeyVault"
    "Microsoft.Storage"
  )
  for provider in "${providers[@]}"; do
    local state
    state=$(az provider show --namespace "$provider" --query "registrationState" -o tsv 2>/dev/null || echo "NotRegistered")
    if [[ "$state" != "Registered" ]]; then
      info "  Registering $provider…"
      az provider register --namespace "$provider" --wait
    else
      info "  $provider – already registered"
    fi
  done
}

# ── Create resource group ─────────────────────────────────────────────────────
create_resource_group() {
  info "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'…"
  az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --tags project=SupremeAI \
    --output none
  info "Resource group ready."
}

# ── Deploy Bicep template ─────────────────────────────────────────────────────
deploy_infrastructure() {
  info "Deploying SupremeAI AI infrastructure (this may take 10-20 minutes)…"

  local params=(
    "baseName=${BASE_NAME}"
    "location=${LOCATION}"
  )
  if [[ -n "$PRINCIPAL_ID" ]]; then
    params+=("secretsAccessPrincipalId=${PRINCIPAL_ID}")
  fi

  DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "${SCRIPT_DIR}/main.bicep" \
    --parameters "${params[@]}" \
    --name "supremeai-deploy-$(date +%Y%m%d%H%M%S)" \
    --query "properties.outputs" \
    --output json)

  info "Deployment complete."
  echo ""
  echo "────────────────────────────────────────────────────────────────────────"
  echo "  Deployment outputs:"
  echo "$DEPLOYMENT_OUTPUT" | python3 -c "
import sys, json
outputs = json.load(sys.stdin)
for k, v in outputs.items():
    print(f'  {k}: {v[\"value\"]}')
" 2>/dev/null || echo "$DEPLOYMENT_OUTPUT"
  echo "────────────────────────────────────────────────────────────────────────"
}

# ── Print next steps ──────────────────────────────────────────────────────────
print_next_steps() {
  echo ""
  info "✅ Setup complete! Next steps:"
  echo ""
  echo "  1. Configure external API keys in Key Vault (for Anthropic, Google, xAI):"
  echo ""
  KV_NAME=$(az keyvault list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv 2>/dev/null || echo "<key-vault-name>")
  echo "     az keyvault secret set --vault-name $KV_NAME --name anthropic-api-key --value <YOUR_KEY>"
  echo "     az keyvault secret set --vault-name $KV_NAME --name google-gemini-api-key --value <YOUR_KEY>"
  echo "     az keyvault secret set --vault-name $KV_NAME --name xai-grok-api-key --value <YOUR_KEY>"
  echo ""
  echo "  2. Set environment variables for the SupremeAI API backend:"
  echo ""
  echo "     export AZURE_KEYVAULT_URI=\$(az keyvault show --name $KV_NAME --query properties.vaultUri -o tsv)"
  echo ""
  echo "  3. Run the SupremeAI API:"
  echo ""
  echo "     cd src/SupremeAI.Api && dotnet run"
  echo ""
  echo "  See scripts/azure/README.md for full documentation."
}

# ── Main ──────────────────────────────────────────────────────────────────────
main() {
  echo "╔══════════════════════════════════════════════════════╗"
  echo "║        SupremeAI – Azure Infrastructure Setup        ║"
  echo "╚══════════════════════════════════════════════════════╝"
  echo ""
  check_prerequisites
  ensure_logged_in
  register_providers
  create_resource_group
  deploy_infrastructure
  print_next_steps
}

main "$@"
