@description('Name of the AI Foundry Hub')
param hubName string

@description('Name of the AI Foundry Project')
param projectName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

@description('Name of the Key Vault to store serverless model endpoints and keys')
param keyVaultName string

@description('Name of a storage account to associate with the AI hub')
param storageAccountName string

// ── Storage account for the AI Hub ──────────────────────────────────────────

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// ── AI Foundry Hub ───────────────────────────────────────────────────────────

resource aiHub 'Microsoft.MachineLearningServices/workspaces@2024-10-01' = {
  name: hubName
  location: location
  tags: tags
  kind: 'Hub'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    friendlyName: hubName
    storageAccount: storageAccount.id
    keyVault: resourceId('Microsoft.KeyVault/vaults', keyVaultName)
    publicNetworkAccess: 'Enabled'
  }
}

// ── AI Foundry Project ───────────────────────────────────────────────────────

resource aiProject 'Microsoft.MachineLearningServices/workspaces@2024-10-01' = {
  name: projectName
  location: location
  tags: tags
  kind: 'Project'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    friendlyName: projectName
    hubResourceId: aiHub.id
    publicNetworkAccess: 'Enabled'
  }
}

// ── Serverless endpoint: Phi-3.5 Mini Instruct ───────────────────────────────

resource phi35MiniEndpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'phi-3-5-mini'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml/models/Phi-3.5-mini-instruct/versions/4'
    }
  }
}

// ── Serverless endpoint: Phi-3 Medium 128k Instruct ─────────────────────────

resource phi3MediumEndpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'phi-3-medium'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml/models/Phi-3-medium-128k-instruct/versions/2'
    }
  }
}

// ── Serverless endpoint: Meta Llama 3.1 70B Instruct ────────────────────────

resource llama31Endpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'llama-3-1-70b'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml-meta/models/Meta-Llama-3.1-70B-Instruct/versions/3'
    }
  }
}

// ── Serverless endpoint: Mistral Large ──────────────────────────────────────

resource mistralLargeEndpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'mistral-large'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml-mistral/models/Mistral-large/versions/1'
    }
  }
}

// ── Serverless endpoint: Cohere Command R+ ──────────────────────────────────

resource commandRPlusEndpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'command-r-plus'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml-cohere/models/Cohere-command-r-plus/versions/1'
    }
  }
}

// ── Serverless endpoint: Jais 30B Chat ──────────────────────────────────────

resource jais30bEndpoint 'Microsoft.MachineLearningServices/workspaces/serverlessEndpoints@2024-10-01' = {
  parent: aiProject
  name: 'jais-30b'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    modelSettings: {
      modelId: 'azureml://registries/azureml-core42/models/jais-30b-chat/versions/1'
    }
  }
}

// ── Store endpoint URLs and keys in Key Vault ────────────────────────────────

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource phi35MiniSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'phi-3-5-mini-endpoint'
  properties: { value: phi35MiniEndpoint.properties.inferenceEndpoint.uri }
}

resource phi35MiniKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'phi-3-5-mini-key'
  properties: { value: phi35MiniEndpoint.listKeys().primaryKey }
}

resource phi3MediumSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'phi-3-medium-endpoint'
  properties: { value: phi3MediumEndpoint.properties.inferenceEndpoint.uri }
}

resource phi3MediumKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'phi-3-medium-key'
  properties: { value: phi3MediumEndpoint.listKeys().primaryKey }
}

resource llama31Secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'llama-3-1-70b-endpoint'
  properties: { value: llama31Endpoint.properties.inferenceEndpoint.uri }
}

resource llama31KeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'llama-3-1-70b-key'
  properties: { value: llama31Endpoint.listKeys().primaryKey }
}

resource mistralSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'mistral-large-endpoint'
  properties: { value: mistralLargeEndpoint.properties.inferenceEndpoint.uri }
}

resource mistralKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'mistral-large-key'
  properties: { value: mistralLargeEndpoint.listKeys().primaryKey }
}

resource commandRPlusSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'command-r-plus-endpoint'
  properties: { value: commandRPlusEndpoint.properties.inferenceEndpoint.uri }
}

resource commandRPlusKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'command-r-plus-key'
  properties: { value: commandRPlusEndpoint.listKeys().primaryKey }
}

resource jais30bSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jais-30b-endpoint'
  properties: { value: jais30bEndpoint.properties.inferenceEndpoint.uri }
}

resource jais30bKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jais-30b-key'
  properties: { value: jais30bEndpoint.listKeys().primaryKey }
}

output aiHubName string = aiHub.name
output aiProjectName string = aiProject.name
output aiProjectId string = aiProject.id
