@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param env_outputs_azure_container_apps_environment_default_domain string

param env_outputs_azure_container_apps_environment_id string

param agent_containerimage string

param agent_containerport string

@allowed(['AzureOpenAI', 'GitHubModels'])
param llmProvider string

param endpoint string

@secure()
param apikey_value string

param model string

param env_outputs_azure_container_registry_endpoint string

param env_outputs_azure_container_registry_managed_identity_id string

var llm_endpoint = llmProvider == 'AzureOpenAI' ? 'https://${replace(replace(endpoint, 'https://', ''), '/', '')}/openai/v1/' : 'https://models.github.ai/inference'
var model_name = llmProvider == 'AzureOpenAI' ? model : 'openai/${model}'

resource agent 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: 'agent'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--chat'
          value: 'Endpoint=${llm_endpoint};Key=${apikey_value};Model=${model_name}'
        }
        {
          name: 'chat-key'
          value: apikey_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(agent_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: env_outputs_azure_container_registry_endpoint
          identity: env_outputs_azure_container_registry_managed_identity_id
        }
      ]
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: agent_containerimage
          name: 'agent'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: agent_containerport
            }
            {
              name: 'ConnectionStrings__chat'
              secretRef: 'connectionstrings--chat'
            }
            {
              name: 'CHAT_URI'
              value: 'https://models.github.ai/inference'
            }
            {
              name: 'CHAT_KEY'
              secretRef: 'chat-key'
            }
            {
              name: 'CHAT_MODELNAME'
              value: 'openai/gpt-5-mini'
            }
            {
              name: 'MCPTODO_HTTP'
              value: 'http://mcptodo.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__mcptodo__http__0'
              value: 'http://mcptodo.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'MCPTODO_HTTPS'
              value: 'https://mcptodo.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__mcptodo__https__0'
              value: 'https://mcptodo.${env_outputs_azure_container_apps_environment_default_domain}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}
