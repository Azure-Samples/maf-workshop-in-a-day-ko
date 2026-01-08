# 00: 개발 환경 설정

이 세션에서는 워크샵에서 사용할 개발 환경을 설정합니다.

## 사전 준비 사항

- 크로미움 계열 웹브라우저 ([Microsoft Edge](https://microsoft.com/edge), [Google Chrome](http://chrome.google.com) 등)
- [Azure 구독](https://azure.microsoft.com/free)
- [GitHub 개인 계정 (무료)](http://github.com/signup) 👉 업무용 계정일 경우 회사 정책에 따라 오작동할 수가 있으니 반드시 개인 계정으로 준비하세요.
- [Microsoft Copilot Studio 평가판 구독](https://go.microsoft.com/fwlink/?LinkId=2107702)

## GitHub Codespaces 열기

이 워크샵은 동일한 개발 환경을 유지하기 위해 [GitHub Codespaces](https://docs.github.com/codespaces)를 활용합니다.

1. 아래 버튼을 클릭해서 새 GitHub Codespaces 인스턴스를 생성하세요.

   [![GitHub Codespaces 인스턴스 생성하기](https://github.com/codespaces/badge.svg)](https://codespaces.new/Azure-Samples/maf-workshop-in-a-day-ko)

1. GitHub Codespaces 인스턴스가 만들어지면 터미널에서 아래 명령어를 하나씩 실행시켜 필요한 환경이 잘 만들어졌는지 확인하세요.

    ```bash
    dotnet --list-sdks
    node --version
    npm --version
    azd version
    az --version
    az bicep version
    aspire --version
    ```

## Azure 로그인

> **NOTE**: Azure 구독을 제공 받았을 경우 진행하세요. 워크샵에 따라 Azure 구독을 제공하지 않을 수도 있습니다.

1. 아래 명령어를 각각 실행시켜 Azure 클라우드에 로그인합니다.

    ```bash
    # Azure Developer CLI 로그인
    azd auth login --use-device-code=true

    # Azure CLI 로그인
    az login --use-device-code
    ```

1. 로그인이 끝나면 아래 명령어를 실행시켜 제대로 로그인했는지 확인합니다.

    ```bash
    # Azure Developer CLI 로그인 확인
    azd auth login --check-status
    
    # Azure CLI 로그인 확인
    az account show
    ```

## Azure OpenAI 인스턴스 생성

> **NOTE**: Azure 구독을 제공 받았을 경우 진행하세요. 워크샵에 따라 Azure 구독을 제공하지 않을 수도 있습니다.

1. 아래 명령어를 실행시켜 Azure OpenAI 인스턴스를 생성하세요.

    ```bash
    azd up
    ```

   아래와 같은 질문이 나오면 적당하게 입력합니다.

   - `? Enter a unique environment name:` 👉 환경 이름 (예: mafworkshop-2026)
   - `? Enter a value for the 'location' infrastructure parameter:` 👉 지역 선택 (예: Australia East)

   잠시 기다리면 Azure OpenAI 인스턴스가 만들어진 것을 확인할 수 있습니다.

1. 아래 명령어를 실행시켜 Azure OpenAI 인스턴스의 엔드포인트와 API 키 값을 확인합니다.

    ```bash
    # zsh/bash
    endpoint=$(azd env get-value 'AZURE_OPENAI_ENDPOINT')
    apiKey=$(az cognitiveservices account keys list --name $(azd env get-value 'AZURE_OPENAI_NAME') --resource-group rg-$(azd env get-value 'AZURE_ENV_NAME') --query "key1" -o tsv)
    ```

    ```powershell
    # PowerShell
    $endpoint = azd env get-value 'AZURE_OPENAI_ENDPOINT'
    $apiKey = az cognitiveservices account keys list --name $(azd env get-value 'AZURE_OPENAI_NAME') --resource-group rg-$(azd env get-value 'AZURE_ENV_NAME') --query "key1" -o tsv
    ```

## GitHub Models 설정

> **NOTE**: 만약 Azure 구독을 사용할 수 없을 경우 [GitHub Models](https://docs.github.com/github-models)에서 제공하는 [gpt-5-mini](https://github.com/marketplace/models/azure-openai/gpt-5-mini) 모델을 무료로 사용할 수 있습니다.

1. [퍼스널 액세스 토큰(PAT)](https://docs.github.com/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)을 생성합니다. 이 때, `models:read` 권한을 주지 않으면 GitHub Models에 접근할 수 없습니다.

1. PAT 생성 후 잘 보관해 둡니다. 한 번 생성한 토큰은 나중에 다시 확인할 수 없으므로 분실할 경우 새로 생성해야 합니다.

---

축하합니다! 워크샵을 진행하기 위한 기본 개발 환경 설정이 끝났습니다. 이제 다음 단계로 이동하세요!

👈 [README](../README.md) | [01: Microsoft Agent Framework 사용해서 에이전트 개발하기](./01-agent-with-maf.md) 👉
