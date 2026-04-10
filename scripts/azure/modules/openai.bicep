@description('Name of the Azure OpenAI account')
param openAiAccountName string

@description('Location for the Azure OpenAI account')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

@description('Name of the Key Vault to store the API key')
param keyVaultName string

@description('Azure OpenAI SKU')
@allowed(['S0'])
param sku string = 'S0'

resource openAiAccount 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: openAiAccountName
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: {
    name: sku
  }
  properties: {
    customSubDomainName: openAiAccountName
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

// ── Model deployments ────────────────────────────────────────────────────────

resource deployGpt4o 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAiAccount
  name: 'gpt-4o'
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-11-20'
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
  }
}

resource deployGpt4oMini 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAiAccount
  name: 'gpt-4o-mini'
  dependsOn: [deployGpt4o]
  sku: {
    name: 'GlobalStandard'
    capacity: 20
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o-mini'
      version: '2024-07-18'
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
  }
}

resource deployO1Preview 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAiAccount
  name: 'o1-preview'
  dependsOn: [deployGpt4oMini]
  sku: {
    name: 'GlobalStandard'
    capacity: 5
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'o1-preview'
      version: '2024-09-12'
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
  }
}

resource deployDalle3 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAiAccount
  name: 'dall-e-3'
  dependsOn: [deployO1Preview]
  sku: {
    name: 'Standard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'dall-e-3'
      version: '3.0'
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
  }
}

// ── Store API key in Key Vault ───────────────────────────────────────────────

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource secretEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'azure-openai-endpoint'
  properties: {
    value: openAiAccount.properties.endpoint
  }
}

resource secretApiKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'azure-openai-api-key'
  properties: {
    value: openAiAccount.listKeys().key1
  }
}

output openAiAccountName string = openAiAccount.name
output openAiEndpoint string = openAiAccount.properties.endpoint
output openAiAccountId string = openAiAccount.id
