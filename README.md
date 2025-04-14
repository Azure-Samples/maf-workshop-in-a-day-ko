# MCP Server: Youtube Subtitles Extractor

This is an MCP server that extracts subtitles from a given YouTube link.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?pivots=programming-language-csharp#install-the-azure-functions-core-tools)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/) with [C# Dev Kit](https://marketplace.visualstudio.com/items/?itemName=ms-dotnettools.csdevkit) and [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- [Docker Desktop](https://docs.docker.com/get-started/get-docker/)

## Getting Started

### Run Azurite for local development

1. Run Azurite

    ```bash
    # bash/zsh
    docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 \
        --name azurite mcr.microsoft.com/azure-storage/azurite
    ```

    ```powershell
    # PowerShell
    docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 `
        --name azurite mcr.microsoft.com/azure-storage/azurite
    ```

### Run Azure Functions MCP server locally

1. Get the repository root

    ```bash
    # bash/zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    ```

1. Run the function app

    ```bash
    cd $REPOSITORY_ROOT/src/McpYouTubeSubtitlesExtractor.FunctionApp
    func start
    ```

### Run Azure Functions MCP server remotely

1. Login to Azure

    ```bash
    # Login with Azure CLI
    az login
    
    # Login with Azure Developer CLI
    azd auth login
    ```

1. Deploy the Function app to Azure

    ```bash
    azd up
    ```

   While provisioning and deploying, you'll be asked to provide subscription ID, location, environment name and vNet

1. After the deployment is complete, get the information by running the following commands:

   - function app name:

     ```bash
     azd env get-value AZURE_FUNCTION_NAME
     ```

   - MCP server access key:

     ```bash
     # bash/zsh
     az functionapp keys list \
         -g rg-$(azd env get-value AZURE_ENV_NAME) \
         -n $(azd env get-value AZURE_FUNCTION_NAME) \
         --query "systemKeys.mcp_extension" -o tsv
     ```

     ```bash
     # PowerShell
     az functionapp keys list `
         -g rg-$(azd env get-value AZURE_ENV_NAME) `
         -n $(azd env get-value AZURE_FUNCTION_NAME) `
         --query "systemKeys.mcp_extension" -o tsv
     ```

### Connect MCP server to an MCP host/client

#### VS Code + Agent Mode + Local MCP server

1. Open Command Palette by typing `F1` or `Ctrl`+`Shift`+`P` on Windows or `Cmd`+`Shift`+`P` on Mac OS, and search `MCP: List Servers`.
1. Choose `mcp-youtube-subtitles-extractor-function-local` then click `Start Server`.
1. Enter prompt like:

    ```text
    Summarise this YouTube video link in 5 bullet points: https://youtu.be/XwnEtZxaokg?si=V39ta45iMni_Uc_m
    ```

1. It will ask you to run `get_available_languages` followed by `get_subtitle`. You might be asked to choose language for the subtitle.
1. Confirm the summary of the video.

#### MCP Inspector + Local MCP server

1. Run MCP Inspector

    ```bash
    npx @modelcontextprotocol/inspector node build/index.js
    ```

1. Open a web browser and navigate to the MCP Inspector web app from the URL displayed by the app (e.g. http://0.0.0.0:5173)
1. Set the transport type to `SSE` 
1. Set the URL to your running Function app's SSE endpoint and **Connect**:

    ```text
    http://0.0.0.0:7071/runtime/webhooks/mcp/sse
    ```

1. Click **List Tools**.
1. Click on a tool and **Run Tool** with a YouTube link and language code like `en` or `ko`.

#### VS Code + Agent Mode + Remote MCP server

1. Open Command Palette by typing `F1` or `Ctrl`+`Shift`+`P` on Windows or `Cmd`+`Shift`+`P` on Mac OS, and search `MCP: List Servers`.
1. Choose `mcp-youtube-subtitles-extractor-function-remote` then click `Start Server`.
1. Enter the function app name.
1. Enter the MCP server access key.
1. Enter prompt like:

    ```text
    Summarise this YouTube video link in 5 bullet points: https://youtu.be/XwnEtZxaokg?si=V39ta45iMni_Uc_m
    ```

1. It will ask you to run `get_available_languages` followed by `get_subtitle`. You might be asked to choose language for the subtitle.
1. Confirm the summary of the video.

#### MCP Inspector + Remote MCP server

1. Run MCP Inspector

    ```bash
    npx @modelcontextprotocol/inspector node build/index.js
    ```

1. Open a web browser and navigate to the MCP Inspector web app from the URL displayed by the app (e.g. http://0.0.0.0:5173)
1. Set the transport type to `SSE` 
1. Set the URL to your running Function app's SSE endpoint and **Connect**:

    ```text
    https://<FUNCTION_APP_NAME>.azurewebsites.net/runtime/webhooks/mcp/sse?code=<MCP_SERVER_ACCESS_KEY>
    ```

1. Click **List Tools**.
1. Click on a tool and **Run Tool** with a YouTube link and language code like `en` or `ko`.
