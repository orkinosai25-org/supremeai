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
| **Azure Web App** | ASP.NET Core 9 app with managed identity and Key Vault integration |

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
```

### 2. Configure the backend API

Set the `AZURE_KEYVAULT_URI` environment variable before running the API:

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

| Variable | Description |
|---|---|
| `AZURE_KEYVAULT_URI` | Key Vault URI – used to auto-load all other secrets |
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI endpoint URL (read from KV if not set) |
| `AZURE_OPENAI_API_KEY` | Azure OpenAI API key (read from KV if not set) |
| `ANTHROPIC_API_KEY` | Anthropic Claude API key |
| `GOOGLE_GEMINI_API_KEY` | Google Gemini API key |
| `XAI_GROK_API_KEY` | xAI Grok API key |

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
    ├── app-service.bicep   # App Service Plan + Web App with managed identity
    ├── key-vault.bicep     # Key Vault with RBAC
    ├── openai.bicep        # Azure OpenAI + model deployments
    └── ai-foundry.bicep    # AI Foundry Hub/Project + serverless endpoints
```
