using './agent-containerapp.module.bicep'

param agent_containerimage = '{{ .Image }}'
param agent_containerport = '{{ targetPortOrDefault 8080 }}'
param apikey_value = '{{ securedParameter "apiKey" }}'
param env_outputs_azure_container_apps_environment_default_domain = '{{ .Env.ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN }}'
param env_outputs_azure_container_apps_environment_id = '{{ .Env.ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID }}'
param env_outputs_azure_container_registry_endpoint = '{{ .Env.ENV_AZURE_CONTAINER_REGISTRY_ENDPOINT }}'
param env_outputs_azure_container_registry_managed_identity_id = '{{ .Env.ENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID }}'
