@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}

@description('The SKU for the Azure OpenAI resource')
@allowed(['S0'])
param sku string = 'S0'

@description('Disallow key-based authentication for the Azure OpenAI resource. Should be disabled in production environments in favor of managed identities')
param disableLocalAuth bool = true

@description('Deploy GPT model automatically')
param deployGptModel bool = true

@description('GPT model to deploy')
param gptModelName string = 'gpt-5-mini'

@description('GPT model version')
param gptModelVersion string = '2025-08-07'

@description('GPT deployment capacity')
param gptCapacity int = 10

var resourceToken = uniqueString(subscription().id, resourceGroup().id, environmentName, location)

// Deploy the Azure OpenAI resource
module openai 'br/public:avm/res/cognitive-services/account:0.14.1' = {
  name: 'openai'
  params: {
    name: 'openai-${resourceToken}'
    location: location
    tags: tags
    kind: 'OpenAI'
    sku: sku
    customSubDomainName: 'openai-${resourceToken}'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    disableLocalAuth: disableLocalAuth
    deployments: [
      {
        name: gptModelName
        model: {
          format: 'OpenAI'
          name: gptModelName
          version: gptModelVersion
        }
        sku: {
          name: 'GlobalStandard'
          capacity: gptCapacity
        }
      }
    ]
  }
}

// Outputs
output AZURE_OPENAI_ENDPOINT string = openai.outputs.endpoint
output AZURE_OPENAI_NAME string = openai.outputs.name
output AZURE_OPENAI_RESOURCE_ID string = openai.outputs.resourceId
output AZURE_OPENAI_GPT_DEPLOYMENT_NAME string = deployGptModel ? gptModelName : ''
