#Requires -Version 5.1
<#
.SYNOPSIS
    SupremeAI – Azure AI infrastructure setup script (PowerShell)

.DESCRIPTION
    Provisions all Azure resources required to run SupremeAI AI models:
      - Azure OpenAI (GPT-4o, GPT-4o-mini, o1-preview, DALL-E 3)
      - Azure AI Foundry Hub + Project with serverless model endpoints
        (Phi-3.5 Mini, Phi-3 Medium, Llama 3.1 70B, Mistral Large, Command R+, Jais 30B)
      - Azure Key Vault for secrets management

.PARAMETER ResourceGroup
    Resource group name (default: supremeai-rg)

.PARAMETER Location
    Azure region (default: eastus)

.PARAMETER BaseName
    Base name for all resources (default: supremeai)

.PARAMETER SubscriptionId
    Azure subscription ID (uses current if omitted)

.PARAMETER PrincipalId
    Object ID of the managed identity or user granted Key Vault secrets access

.EXAMPLE
    .\setup.ps1 -ResourceGroup myRg -Location westeurope -BaseName myapp
#>

[CmdletBinding()]
param(
    [string]$ResourceGroup   = "supremeai-rg",
    [string]$Location        = "eastus",
    [string]$BaseName        = "supremeai",
    [string]$SubscriptionId  = "",
    [string]$PrincipalId     = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Write-Info  { param([string]$msg) Write-Host "[INFO]  $msg" -ForegroundColor Green  }
function Write-Warn  { param([string]$msg) Write-Host "[WARN]  $msg" -ForegroundColor Yellow }
function Write-Err   { param([string]$msg) Write-Host "[ERROR] $msg" -ForegroundColor Red    }

# ── Check prerequisites ───────────────────────────────────────────────────────
function Test-Prerequisites {
    Write-Info "Checking prerequisites…"
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        Write-Err "Azure CLI (az) is not installed."
        Write-Err "Install from: https://aka.ms/installazurecliwindows"
        exit 1
    }
    $version = (az version --query '"azure-cli"' -o tsv 2>$null)
    Write-Info "Azure CLI version: $version"
}

# ── Ensure logged in ──────────────────────────────────────────────────────────
function Confirm-Login {
    Write-Info "Checking Azure login…"
    $account = az account show 2>$null | ConvertFrom-Json
    if (-not $account) {
        Write-Warn "Not logged in. Initiating login…"
        az login | Out-Null
    }
    if ($SubscriptionId) {
        az account set --subscription $SubscriptionId | Out-Null
        Write-Info "Using subscription: $SubscriptionId"
    } else {
        $script:SubscriptionId = (az account show --query id -o tsv)
        Write-Info "Using current subscription: $($script:SubscriptionId)"
    }
}

# ── Register resource providers ───────────────────────────────────────────────
function Register-Providers {
    Write-Info "Registering required Azure resource providers…"
    $providers = @(
        "Microsoft.CognitiveServices",
        "Microsoft.MachineLearningServices",
        "Microsoft.KeyVault",
        "Microsoft.Storage",
        "Microsoft.Web"
    )
    foreach ($p in $providers) {
        $state = az provider show --namespace $p --query "registrationState" -o tsv 2>$null
        if ($state -ne "Registered") {
            Write-Info "  Registering $p…"
            az provider register --namespace $p --wait | Out-Null
        } else {
            Write-Info "  $p – already registered"
        }
    }
}

# ── Create resource group ─────────────────────────────────────────────────────
function New-ResourceGroup {
    Write-Info "Creating resource group '$ResourceGroup' in '$Location'…"
    az group create `
        --name $ResourceGroup `
        --location $Location `
        --tags project=SupremeAI `
        --output none
    Write-Info "Resource group ready."
}

# ── Deploy Bicep template ─────────────────────────────────────────────────────
function Deploy-Infrastructure {
    Write-Info "Deploying SupremeAI AI infrastructure (this may take 10-20 minutes)…"

    $paramArgs = @(
        "baseName=$BaseName",
        "location=$Location"
    )
    if ($PrincipalId) {
        $paramArgs += "secretsAccessPrincipalId=$PrincipalId"
    }

    $timestamp = Get-Date -Format "yyyyMMddHHmmss"
    $output = az deployment group create `
        --resource-group $ResourceGroup `
        --template-file "$ScriptDir\main.bicep" `
        --parameters @paramArgs `
        --name "supremeai-deploy-$timestamp" `
        --query "properties.outputs" `
        --output json 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Err "Deployment failed:`n$output"
        exit 1
    }

    Write-Info "Deployment complete."
    Write-Host ""
    Write-Host ("─" * 72)
    Write-Host "  Deployment outputs:"
    try {
        $parsed = $output | ConvertFrom-Json
        $parsed.PSObject.Properties | ForEach-Object {
            Write-Host "  $($_.Name): $($_.Value.value)"
        }
    } catch {
        Write-Host $output
    }
    Write-Host ("─" * 72)
}

# ── Print next steps ──────────────────────────────────────────────────────────
function Show-NextSteps {
    Write-Host ""
    Write-Info "✅ Setup complete! Next steps:"
    Write-Host ""

    $kvName = az keyvault list --resource-group $ResourceGroup --query "[0].name" -o tsv 2>$null
    if (-not $kvName) { $kvName = "<key-vault-name>" }

    Write-Host "  1. Add external API keys to Key Vault (Anthropic, Google, xAI):"
    Write-Host ""
    Write-Host "     az keyvault secret set --vault-name $kvName --name anthropic-api-key    --value <KEY>"
    Write-Host "     az keyvault secret set --vault-name $kvName --name google-gemini-api-key --value <KEY>"
    Write-Host "     az keyvault secret set --vault-name $kvName --name xai-grok-api-key     --value <KEY>"
    Write-Host ""
    Write-Host "  2. Set environment variable for the SupremeAI API backend:"
    Write-Host ""
    Write-Host '     $env:AZURE_KEYVAULT_URI = (az keyvault show --name ' + $kvName + ' --query properties.vaultUri -o tsv)'
    Write-Host ""
    Write-Host "  3. Run the SupremeAI API:"
    Write-Host ""
    Write-Host "     cd src\SupremeAI.Api; dotnet run"
    Write-Host ""
    Write-Host "  See scripts\azure\README.md for full documentation."
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Host "╔══════════════════════════════════════════════════════╗"
Write-Host "║        SupremeAI – Azure Infrastructure Setup        ║"
Write-Host "╚══════════════════════════════════════════════════════╝"
Write-Host ""

Test-Prerequisites
Confirm-Login
Register-Providers
New-ResourceGroup
Deploy-Infrastructure
Show-NextSteps
