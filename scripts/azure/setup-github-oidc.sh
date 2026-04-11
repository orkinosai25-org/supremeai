#!/usr/bin/env bash
# SupremeAI – GitHub OIDC Setup Script
#
# Automates the one-time Azure service-principal and GitHub OIDC Workload
# Identity Federation setup so that the provision-and-deploy workflow can
# run without any manual Azure Portal steps or PUBLISH_PROFILE downloads.
#
# What this script does:
#   1. Logs in to Azure (prompts if not already logged in)
#   2. Creates an Azure AD application + service principal (or reuses existing)
#   3. Assigns Contributor + Key Vault Secrets Officer roles on the subscription
#   4. Creates a federated identity credential for GitHub Actions OIDC
#   5. Uses the GitHub CLI (gh) to set AZURE_CLIENT_ID, AZURE_TENANT_ID,
#      AZURE_SUBSCRIPTION_ID as GitHub repository secrets automatically
#
# After this script completes, push to the main branch – everything else is
# fully automated.
#
# Usage:
#   chmod +x scripts/azure/setup-github-oidc.sh
#   ./scripts/azure/setup-github-oidc.sh --repo <owner/repo>
#
# Options:
#   --repo          <owner/repo>  GitHub repository (e.g. myorg/supremeai)  [required]
#   --app-name      <name>        Azure AD app display name  (default: supremeai-github-actions)
#   --subscription  <id>          Azure subscription ID      (default: current subscription)
#   --branch        <branch>      Branch to allow in OIDC subject (default: main)
#   --help                        Show this help message
#
# Prerequisites:
#   • Azure CLI (az)  – https://aka.ms/installazurecliwindows
#   • GitHub CLI (gh) – https://cli.github.com
#   • Owner or Contributor + User Access Administrator on the Azure subscription

set -euo pipefail

# ── Defaults ─────────────────────────────────────────────────────────────────
REPO=""
APP_NAME="supremeai-github-actions"
SUBSCRIPTION_ID=""
BRANCH="main"

# ── Colour helpers ────────────────────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; RED='\033[0;31m'; BOLD='\033[1m'; NC='\033[0m'
info()    { echo -e "${GREEN}[INFO]${NC} $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC} $*"; }
error()   { echo -e "${RED}[ERROR]${NC} $*" >&2; }
header()  { echo -e "\n${BOLD}${CYAN}── $* ──${NC}"; }
ok()      { echo -e "  ${GREEN}✓${NC} $*"; }

# ── Parse arguments ───────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo)         REPO="$2";          shift 2 ;;
    --app-name)     APP_NAME="$2";      shift 2 ;;
    --subscription) SUBSCRIPTION_ID="$2"; shift 2 ;;
    --branch)       BRANCH="$2";        shift 2 ;;
    --help)
      sed -n '/^# Usage/,/^[^#]/{ /^[^#]/d; s/^# \?//; p }' "$0"
      exit 0 ;;
    *) error "Unknown argument: $1"; exit 1 ;;
  esac
done

if [[ -z "$REPO" ]]; then
  # Try to detect the repo from the local git remote
  DETECTED=$(git remote get-url origin 2>/dev/null \
    | sed -E 's|.*github\.com[:/]([^/]+/[^/.]+)(\.git)?$|\1|' || true)
  if [[ -n "$DETECTED" ]]; then
    warn "No --repo provided. Detected from git remote: $DETECTED"
    REPO="$DETECTED"
  else
    error "Usage: $0 --repo <owner/repo>  (e.g. myorg/supremeai)"
    exit 1
  fi
fi

# ── Banner ────────────────────────────────────────────────────────────────────
echo ""
echo -e "${BOLD}${CYAN}╔══════════════════════════════════════════════════════╗${NC}"
echo -e "${BOLD}${CYAN}║   SupremeAI – GitHub OIDC Zero-Setup Script          ║${NC}"
echo -e "${BOLD}${CYAN}╚══════════════════════════════════════════════════════╝${NC}"
echo ""
echo "  Repository : $REPO"
echo "  App name   : $APP_NAME"
echo "  Branch     : $BRANCH"
echo ""

# ── 1. Prerequisites ──────────────────────────────────────────────────────────
header "Step 1 – Checking prerequisites"

for tool in az gh; do
  if ! command -v "$tool" &>/dev/null; then
    error "$tool is not installed."
    case "$tool" in
      az) echo "  Install from: https://aka.ms/installazurecliwindows (Windows) or 'brew install azure-cli' (macOS)" ;;
      gh) echo "  Install from: https://cli.github.com (or 'brew install gh')" ;;
    esac
    exit 1
  fi
done
ok "Azure CLI $(az version --query '"azure-cli"' -o tsv 2>/dev/null)"
ok "GitHub CLI $(gh --version | head -1)"

# ── 2. Azure login ────────────────────────────────────────────────────────────
header "Step 2 – Azure login"

if ! az account show &>/dev/null; then
  info "Not logged in. Initiating login…"
  az login
fi

if [[ -n "$SUBSCRIPTION_ID" ]]; then
  az account set --subscription "$SUBSCRIPTION_ID"
else
  SUBSCRIPTION_ID=$(az account show --query id -o tsv)
fi
TENANT_ID=$(az account show --query tenantId -o tsv)
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
ok "Subscription: $SUBSCRIPTION_NAME ($SUBSCRIPTION_ID)"
ok "Tenant: $TENANT_ID"

# ── 3. GitHub CLI login ───────────────────────────────────────────────────────
header "Step 3 – GitHub login"

if ! gh auth status &>/dev/null; then
  info "Not logged in to GitHub. Initiating login…"
  gh auth login
fi
ok "GitHub CLI is authenticated"

# ── 4. Create or reuse Azure AD application + service principal ──────────────
header "Step 4 – Azure AD application and service principal"

# Check if an application with this name already exists
EXISTING_APP_ID=$(az ad app list \
  --display-name "$APP_NAME" \
  --query "[0].appId" -o tsv 2>/dev/null || true)

if [[ -n "$EXISTING_APP_ID" && "$EXISTING_APP_ID" != "None" ]]; then
  CLIENT_ID="$EXISTING_APP_ID"
  ok "Reusing existing Azure AD application: $APP_NAME ($CLIENT_ID)"
else
  CLIENT_ID=$(az ad app create \
    --display-name "$APP_NAME" \
    --query appId -o tsv)
  ok "Created Azure AD application: $APP_NAME ($CLIENT_ID)"
fi

# Create service principal if it doesn't exist
SP_EXISTS=$(az ad sp show --id "$CLIENT_ID" --query id -o tsv 2>/dev/null || true)
if [[ -z "$SP_EXISTS" || "$SP_EXISTS" == "None" ]]; then
  az ad sp create --id "$CLIENT_ID" --output none
  ok "Created service principal for $APP_NAME"
else
  ok "Service principal already exists"
fi

SP_OBJECT_ID=$(az ad sp show --id "$CLIENT_ID" --query id -o tsv)

# ── 5. Assign roles ───────────────────────────────────────────────────────────
header "Step 5 – Role assignments"

SCOPE="/subscriptions/$SUBSCRIPTION_ID"

assign_role() {
  local role="$1"
  local scope="$2"
  local existing
  existing=$(az role assignment list \
    --assignee "$SP_OBJECT_ID" \
    --role     "$role" \
    --scope    "$scope" \
    --query    "[0].id" -o tsv 2>/dev/null || true)

  if [[ -n "$existing" && "$existing" != "None" ]]; then
    ok "'$role' already assigned (scope: $scope)"
  else
    az role assignment create \
      --role               "$role" \
      --assignee-object-id "$SP_OBJECT_ID" \
      --assignee-principal-type ServicePrincipal \
      --scope              "$scope" \
      --output             none
    ok "Assigned '$role' (scope: $scope)"
  fi
}

# Contributor: lets CI create/update all Azure resources (Bicep deploys)
assign_role "Contributor" "$SCOPE"

# Key Vault Secrets Officer: lets CI store external API keys in Key Vault
assign_role "Key Vault Secrets Officer" "$SCOPE"

# ── 6. Create federated credential ───────────────────────────────────────────
header "Step 6 – OIDC federated credentials"

# Subject format for GitHub Actions push/dispatch on a branch:
#   repo:<owner>/<repo>:ref:refs/heads/<branch>
SUBJECT_BRANCH="repo:${REPO}:ref:refs/heads/${BRANCH}"

# Also allow manual workflow_dispatch from any ref (environment: production)
SUBJECT_DISPATCH="repo:${REPO}:environment:production"

create_federated_cred() {
  local name="$1"
  local subject="$2"
  local issuer="https://token.actions.githubusercontent.com"

  # Check if it already exists
  local existing
  existing=$(az ad app federated-credential list \
    --id "$CLIENT_ID" \
    --query "[?subject=='${subject}'].id" -o tsv 2>/dev/null || true)

  if [[ -n "$existing" ]]; then
    ok "Federated credential '$name' already exists"
  else
    az ad app federated-credential create \
      --id         "$CLIENT_ID" \
      --parameters "{
        \"name\": \"$name\",
        \"issuer\": \"$issuer\",
        \"subject\": \"$subject\",
        \"description\": \"GitHub Actions OIDC for SupremeAI (${name})\",
        \"audiences\": [\"api://AzureADTokenExchange\"]
      }" \
      --output none
    ok "Created federated credential '$name' for subject: $subject"
  fi
}

create_federated_cred "github-actions-${BRANCH}" "$SUBJECT_BRANCH"
create_federated_cred "github-actions-dispatch"   "$SUBJECT_DISPATCH"

# ── 7. Set GitHub repository secrets ─────────────────────────────────────────
header "Step 7 – Setting GitHub repository secrets"

set_secret() {
  local name="$1"
  local value="$2"
  printf '%s' "$value" | gh secret set "$name" --repo "$REPO"
  ok "Set GitHub secret: $name"
}

set_secret "AZURE_CLIENT_ID"       "$CLIENT_ID"
set_secret "AZURE_TENANT_ID"       "$TENANT_ID"
set_secret "AZURE_SUBSCRIPTION_ID" "$SUBSCRIPTION_ID"

# ── Done ──────────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}${BOLD}╔══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}${BOLD}║   ✅  One-time OIDC setup complete!                  ║${NC}"
echo -e "${GREEN}${BOLD}╚══════════════════════════════════════════════════════╝${NC}"
echo ""
echo "  The following GitHub secrets have been set on $REPO:"
echo "    • AZURE_CLIENT_ID       = $CLIENT_ID"
echo "    • AZURE_TENANT_ID       = $TENANT_ID"
echo "    • AZURE_SUBSCRIPTION_ID = $SUBSCRIPTION_ID"
echo ""
echo "  Optional: add external AI provider keys as GitHub secrets so the"
echo "  provision-and-deploy workflow can store them in Key Vault automatically:"
echo ""
echo "    gh secret set ANTHROPIC_API_KEY     --repo $REPO"
echo "    gh secret set GOOGLE_GEMINI_API_KEY --repo $REPO"
echo "    gh secret set XAI_GROK_API_KEY      --repo $REPO"
echo ""
echo "  Now push to the '$BRANCH' branch — Azure infrastructure will be"
echo "  provisioned and the app deployed automatically with no further manual steps."
echo ""
echo "  To verify, watch the 'Provision and Deploy SupremeAI' workflow at:"
echo "  https://github.com/$REPO/actions"
echo ""
