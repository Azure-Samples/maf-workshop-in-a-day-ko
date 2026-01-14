targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

@allowed(['AzureOpenAI', 'GitHubModels'])
@description('The LLM provider to use for the application')
param aiLlmProvider string

@description('The endpoint for the LLM service')
param endpoint string

@secure()
@description('The API key for the LLM service')
param apiKey string

param model string = 'gpt-5-mini'

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module env 'env/env.module.bicep' = {
  name: 'env'
  scope: rg
  params: {
    env_acr_outputs_name: env_acr.outputs.name
    location: location
    userPrincipalId: principalId
  }
}

module env_acr 'env-acr/env-acr.module.bicep' = {
  name: 'env-acr'
  scope: rg
  params: {
    location: location
  }
}

module agent 'agent/agent-containerapp.module.bicep' = {
  name: 'agent'
  scope: rg
  params: {
    location: location
    env_outputs_azure_container_apps_environment_default_domain: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    env_outputs_azure_container_apps_environment_id: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    agent_containerimage: '${env_acr.outputs.loginServer}/agent:latest'
    agent_containerport: '8080'
    llmProvider: aiLlmProvider
    endpoint: endpoint
    apikey_value: apiKey
    model: model
    env_outputs_azure_container_registry_endpoint: env_acr.outputs.loginServer
    env_outputs_azure_container_registry_managed_identity_id: env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
  }
}

module mcptodo 'mcptodo/mcptodo-containerapp.module.bicep' = {
  name: 'mcptodo'
  scope: rg
  params: {
    location: location
    env_outputs_azure_container_apps_environment_default_domain: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    env_outputs_azure_container_apps_environment_id: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    mcptodo_containerimage: '${env_acr.outputs.loginServer}/mcptodo:latest'
    mcptodo_containerport: '8080'
    env_outputs_azure_container_registry_endpoint: env_acr.outputs.loginServer
    env_outputs_azure_container_registry_managed_identity_id: env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
  }
}

module webui 'webui/webui-containerapp.module.bicep' = {
  name: 'webui'
  scope: rg
  params: {
    location: location
    env_outputs_azure_container_apps_environment_default_domain: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    env_outputs_azure_container_apps_environment_id: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    webui_containerimage: '${env_acr.outputs.loginServer}/webui:latest'
    webui_containerport: '8080'
    env_outputs_azure_container_registry_endpoint: env_acr.outputs.loginServer
    env_outputs_azure_container_registry_managed_identity_id: env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
  }
}

output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output ENV_AZURE_CONTAINER_REGISTRY_ENDPOINT string = env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output ENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
