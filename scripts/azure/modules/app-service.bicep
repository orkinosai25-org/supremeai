@description('Name of the App Service Plan')
param appServicePlanName string

@description('Name of the Web App')
param webAppName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

@description('URI of the Key Vault – written to AZURE_KEYVAULT_URI app setting')
param keyVaultUri string = ''

@description('Azure OpenAI endpoint – written to AZURE_OPENAI_ENDPOINT app setting')
param openAiEndpoint string = ''

@description('Name of the Key Vault to grant the Web App managed identity secrets access')
param keyVaultName string = ''

// ── App Service Plan (Linux B1) ──────────────────────────────────────────────

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true // required for Linux plans
  }
}

// ── Web App (ASP.NET Core 10 on Linux) ───────────────────────────────────────

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT',   value: 'Production' }
        { name: 'WEBSITES_PORT',            value: '8080' }
        { name: 'AZURE_KEYVAULT_URI',       value: keyVaultUri }

        // ── Azure OpenAI ─────────────────────────────────────────────────────
        // Key Vault references: App Service resolves these at runtime using the
        // web app's system-assigned managed identity (Key Vault Secrets User role).
        { name: 'AZURE_OPENAI_ENDPOINT', value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-openai-endpoint)' : '' }
        { name: 'AZURE_OPENAI_API_KEY',  value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-openai-api-key)'  : '' }

        // ── External AI providers ─────────────────────────────────────────────
        { name: 'ANTHROPIC_API_KEY',      value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=anthropic-api-key)'      : '' }
        { name: 'GOOGLE_GEMINI_API_KEY',  value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=google-gemini-api-key)'  : '' }
        { name: 'XAI_GROK_API_KEY',       value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=xai-grok-api-key)'       : '' }
        { name: 'OPENAI_API_KEY',         value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=openai-api-key)'         : '' }

        // ── Azure AI Foundry serverless endpoints ─────────────────────────────
        { name: 'PHI_35_MINI_ENDPOINT',     value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=phi-3-5-mini-endpoint)'     : '' }
        { name: 'PHI_35_MINI_API_KEY',      value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=phi-3-5-mini-key)'          : '' }
        { name: 'PHI_3_MEDIUM_ENDPOINT',    value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=phi-3-medium-endpoint)'     : '' }
        { name: 'PHI_3_MEDIUM_API_KEY',     value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=phi-3-medium-key)'          : '' }
        { name: 'LLAMA_31_70B_ENDPOINT',    value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=llama-3-1-70b-endpoint)'    : '' }
        { name: 'LLAMA_31_70B_API_KEY',     value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=llama-3-1-70b-key)'         : '' }
        { name: 'MISTRAL_LARGE_ENDPOINT',   value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=mistral-large-endpoint)'    : '' }
        { name: 'MISTRAL_LARGE_API_KEY',    value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=mistral-large-key)'         : '' }
        { name: 'COMMAND_R_PLUS_ENDPOINT',  value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=command-r-plus-endpoint)'   : '' }
        { name: 'COMMAND_R_PLUS_API_KEY',   value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=command-r-plus-key)'        : '' }
        { name: 'JAIS_30B_ENDPOINT',        value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=jais-30b-endpoint)'         : '' }
        { name: 'JAIS_30B_API_KEY',         value: !empty(keyVaultName) ? '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=jais-30b-key)'              : '' }
      ]
    }
  }
}

// ── Grant Key Vault Secrets User role to the Web App managed identity ────────
// Role: Key Vault Secrets User (4633458b-17de-408a-b874-0445c86b69e6)

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = if (!empty(keyVaultName)) {
  name: keyVaultName
}

resource kvSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(keyVaultName)) {
  name: guid(keyVault.id, webApp.id, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────

output appServicePlanName string = appServicePlan.name
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output principalId string = webApp.identity.principalId
