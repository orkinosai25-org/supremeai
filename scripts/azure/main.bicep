@description('Base name for all resources (e.g. supremeai-prod)')
param baseName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Object ID of the managed identity or user that needs Key Vault secrets access')
param secretsAccessPrincipalId string = ''

var tags = {
  project: 'SupremeAI'
  environment: 'production'
}

var keyVaultName = '${take(replace(baseName, '-', ''), 20)}kv'
var openAiAccountName = '${baseName}-oai'
var aiHubName = '${baseName}-hub'
var aiProjectName = '${baseName}-project'
var storageAccountName = '${take(replace(baseName, '-', ''), 18)}sa'

// ── Key Vault ────────────────────────────────────────────────────────────────

module keyVaultModule 'modules/key-vault.bicep' = {
  name: 'keyVaultDeploy'
  params: {
    keyVaultName: keyVaultName
    location: location
    tags: tags
    secretsAccessPrincipalId: secretsAccessPrincipalId
  }
}

// ── Azure OpenAI ─────────────────────────────────────────────────────────────

module openAiModule 'modules/openai.bicep' = {
  name: 'openAiDeploy'
  params: {
    openAiAccountName: openAiAccountName
    location: location
    tags: tags
    keyVaultName: keyVaultName
  }
  dependsOn: [keyVaultModule]
}

// ── Azure AI Foundry (Hub + Project + Serverless models) ─────────────────────

module aiFoundryModule 'modules/ai-foundry.bicep' = {
  name: 'aiFoundryDeploy'
  params: {
    hubName: aiHubName
    projectName: aiProjectName
    storageAccountName: storageAccountName
    location: location
    tags: tags
    keyVaultName: keyVaultName
  }
  dependsOn: [keyVaultModule]
}

// ── Outputs ──────────────────────────────────────────────────────────────────

output keyVaultName string = keyVaultModule.outputs.keyVaultName
output keyVaultUri string = keyVaultModule.outputs.keyVaultUri
output openAiEndpoint string = openAiModule.outputs.openAiEndpoint
output aiProjectName string = aiFoundryModule.outputs.aiProjectName
