# Azure AI Infrastructure – SupremeAI

This directory contains **Bicep templates** and **setup scripts** to provision
all Azure resources required to run SupremeAI's AI model integrations.

## What gets provisioned

| Resource | Purpose |
|---|---|
| **Azure Key Vault** | Centrally stores all API keys and endpoint URLs |
| **Azure OpenAI** | Hosts GPT-4o, GPT-4o-mini, o1-preview, DALL-E 3 |
| **Azure AI Foundry Hub** | Organises all AI Foundry resources |
| **Azure AI Foundry Project** | Serverless model endpoints |
| **Serverless endpoints** | Phi-3.5 Mini, Phi-3 Medium 128k, Llama 3.1 70B, Mistral Large, Command R+, Jais 30B |
| **Storage Account** | Required by AI Foundry Hub |
| **App Service Plan** | Linux B1 hosting plan for the backend API |
| **Azure Web App** | ASP.NET Core 10 app with managed identity and Key Vault integration |

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) ≥ 2.55
- An Azure subscription with sufficient quota in your target region
- Contributor + User Access Administrator roles on the subscription
  (needed to assign Key Vault RBAC roles)

## Quick Start

### Bash (Linux / macOS / WSL)

```bash
cd scripts/azure
chmod +x setup.sh
./setup.sh
```

### PowerShell (Windows / cross-platform)

```powershell
cd scripts\azure
.\setup.ps1
```

### Custom parameters

```bash
# Bash
./setup.sh \
  --resource-group my-rg \
  --location westeurope \
  --base-name myapp \
  --principal-id <managed-identity-object-id>

# PowerShell
.\setup.ps1 `
  -ResourceGroup my-rg `
  -Location westeurope `
  -BaseName myapp `
  -PrincipalId <managed-identity-object-id>
```

### Deploy Bicep directly with Azure CLI

```bash
az group create --name supremeai-rg --location eastus

az deployment group create \
  --resource-group supremeai-rg \
  --template-file scripts/azure/main.bicep \
  --parameters baseName=supremeai location=eastus
```

## After deployment

### 1. Add external provider API keys

The Bicep templates provision Azure-hosted models automatically.  
For **Anthropic**, **Google**, and **xAI** you must supply your own API keys:

```bash
KV_NAME=$(az keyvault list -g supremeai-rg --query "[0].name" -o tsv)

az keyvault secret set --vault-name $KV_NAME \
  --name anthropic-api-key --value "sk-ant-..."

az keyvault secret set --vault-name $KV_NAME \
  --name google-gemini-api-key --value "AIza..."

az keyvault secret set --vault-name $KV_NAME \
  --name xai-grok-api-key --value "xai-..."

# Optional: direct OpenAI API key (for future use)
az keyvault secret set --vault-name $KV_NAME \
  --name openai-api-key --value "sk-..."
```

### 2. App Service Key Vault references

The App Service is configured with **Key Vault references** for all AI provider
credentials. The web app's system-assigned managed identity is granted the
`Key Vault Secrets User` role so it can resolve these references at runtime.

This means environment variables like `ANTHROPIC_API_KEY`, `GOOGLE_GEMINI_API_KEY`,
`AZURE_OPENAI_API_KEY`, etc. are automatically populated from Key Vault — **no
manual app setting updates are needed** after storing the secrets.

Example App Setting value set by Bicep:
```
ANTHROPIC_API_KEY = @Microsoft.KeyVault(VaultName=supremeaikv;SecretName=anthropic-api-key)
```

### 3. Configure the backend API (local development)

Set the `AZURE_KEYVAULT_URI` environment variable before running the API locally:

```bash
export AZURE_KEYVAULT_URI=$(az keyvault show --name $KV_NAME \
  --query properties.vaultUri -o tsv)

cd src/SupremeAI.Api
dotnet run
```

Or via `appsettings.json` / environment variable in your hosting platform:

```json
{
  "AzureKeyVaultUri": "https://<kv-name>.vault.azure.net/"
}
```

## Environment variables reference

The `SupremeAI.Api` backend reads the following environment variables
(all are optional – missing keys cause that provider to return an error):

| Variable | Key Vault Secret Name | Description |
|---|---|---|
| `AZURE_KEYVAULT_URI` | *(not in KV)* | Key Vault URI – used to auto-load all other secrets in local dev |
| `AZURE_OPENAI_ENDPOINT` | `azure-openai-endpoint` | Azure OpenAI endpoint URL |
| `AZURE_OPENAI_API_KEY` | `azure-openai-api-key` | Azure OpenAI API key |
| `OPENAI_API_KEY` | `openai-api-key` | Direct OpenAI API key (optional, for future use) |
| `ANTHROPIC_API_KEY` | `anthropic-api-key` | Anthropic Claude API key |
| `GOOGLE_GEMINI_API_KEY` | `google-gemini-api-key` | Google Gemini API key |
| `XAI_GROK_API_KEY` | `xai-grok-api-key` | xAI Grok API key |
| `PHI_35_MINI_ENDPOINT` | `phi-3-5-mini-endpoint` | Azure AI – Phi-3.5 Mini endpoint |
| `PHI_35_MINI_API_KEY` | `phi-3-5-mini-key` | Azure AI – Phi-3.5 Mini API key |
| `PHI_3_MEDIUM_ENDPOINT` | `phi-3-medium-endpoint` | Azure AI – Phi-3 Medium endpoint |
| `PHI_3_MEDIUM_API_KEY` | `phi-3-medium-key` | Azure AI – Phi-3 Medium API key |
| `LLAMA_31_70B_ENDPOINT` | `llama-3-1-70b-endpoint` | Azure AI – Llama 3.1 70B endpoint |
| `LLAMA_31_70B_API_KEY` | `llama-3-1-70b-key` | Azure AI – Llama 3.1 70B API key |
| `MISTRAL_LARGE_ENDPOINT` | `mistral-large-endpoint` | Azure AI – Mistral Large endpoint |
| `MISTRAL_LARGE_API_KEY` | `mistral-large-key` | Azure AI – Mistral Large API key |
| `COMMAND_R_PLUS_ENDPOINT` | `command-r-plus-endpoint` | Azure AI – Command R+ endpoint |
| `COMMAND_R_PLUS_API_KEY` | `command-r-plus-key` | Azure AI – Command R+ API key |
| `JAIS_30B_ENDPOINT` | `jais-30b-endpoint` | Azure AI – Jais 30B endpoint |
| `JAIS_30B_API_KEY` | `jais-30b-key` | Azure AI – Jais 30B API key |

## Supported regions

Azure OpenAI model availability varies by region. Recommended regions:

| Region | GPT-4o | o1-preview | DALL-E 3 | Notes |
|---|---|---|---|---|
| `eastus` | ✅ | ✅ | ✅ | Best availability |
| `westeurope` | ✅ | ✅ | ✅ | EU data residency |
| `australiaeast` | ✅ | ❌ | ✅ | APAC |
| `swedencentral` | ✅ | ✅ | ✅ | EU, high quota |

See [Azure OpenAI region availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models#model-summary-table-and-region-availability) for the latest list.

## File structure

```
scripts/azure/
├── README.md           # This file
├── main.bicep          # Root deployment template
├── setup.sh            # Bash setup script
├── setup.ps1           # PowerShell setup script
└── modules/
    ├── app-service.bicep   # App Service Plan + Web App with managed identity + KV references
    ├── key-vault.bicep     # Key Vault with RBAC
    ├── openai.bicep        # Azure OpenAI + model deployments
    └── ai-foundry.bicep    # AI Foundry Hub/Project + serverless endpoints
```
