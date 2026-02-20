# 01: Microsoft Agent Framework 사용해서 단일 에이전트 개발하기

이 세션에서는 Microsoft Agent Framework를 사용해서 단일 에이전트 백엔드 애플리케이션을 개발합니다.

## 세션 목표

- Microsoft Agent Framework에 다양한 LLM을 연결할 수 있습니다.
- Microsoft Agent Framework에 단일 에이전트를 붙일 수 있습니다.
- Microsoft Agent Framework에서 동작하는 에이전트의 흐름을 시각화할 수 있습니다.

## 아키텍처

이 세션이 끝나고 나면 아래와 같은 시스템이 만들어집니다.

![세션 아키텍처](./images/step-01-architecture.png)

## 사전 준비 사항

이전 [00: 개발 환경 설정하기](./00-setup.md)에서 개발 환경을 모두 설정한 상태라고 가정합니다.

## 리포지토리 루트 설정

1. 아래 명령어를 실행시켜 `$REPOSITORY_ROOT` 환경 변수를 설정합니다.

    ```bash
    # zsh/bash
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    ```

## 시작 프로젝트 복사

이 워크샵을 위해 필요한 시작 프로젝트를 준비해 뒀습니다. 시작 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
save-points/
└── step-01/
    └── start/
        ├── MafWorkshop.sln
        └── MafWorkshop.Agent/
            ├── Properties/
            │   └── launchSettings.json
            ├── Program.cs
            ├── appsettings.json
            └── MafWorkshop.Agent.csproj
```

> 프로젝트 소개:
>
> - `MafWorkshop.Agent`: 백엔드 에이전트 애플리케이션 프로젝트

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # zsh/bash
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-01/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-01/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

## LLM 접근 권한 설정

이전 [00: 개발 환경 설정](./00-setup.md)에서 GitHub Models 접근을 위한 PAT과 Azure OpenAI 인스턴스 생성 후 접근을 위한 API 키를 생성했습니다. 이를 애플리케이션에서 사용할 수 있도록 합니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 아래 명령어를 실행시켜 앞서 생성한 값을 저장합니다.

    ```bash
    # GitHub Models
    dotnet user-secrets --project ./MafWorkshop.Agent set GitHub:Token $githubToken
    ```

   아래는 Azure 구독이 있는 경우에만 실행하세요.

    ```bash
    # Azure OpenAI
    dotnet user-secrets --project ./MafWorkshop.Agent set Azure:OpenAI:Endpoint $endpoint
    dotnet user-secrets --project ./MafWorkshop.Agent set Azure:OpenAI:ApiKey $apiKey
    ```

## 시작 프로젝트 빌드 및 실행

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 404 에러 페이지가 나타나는지 확인합니다.

   ![404 에러페이지](./images/step-01-image-01.png)

   현재 아무것도 추가하지 않았으므로 당연하게 404 에러 페이지가 나타나야 합니다.

1. 터미널에서 `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## LLM 연결

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/appsettings.json` 파일을 열고 `LlmProvider` 값이 `GitHubModels`인지 확인합니다. 만약 다른 값으로 되어 있으면 `GitHubModels`로 변경합니다.

    ```jsonc
    {
      "LlmProvider": "GitHubModels"
    }
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// ChatClientFactory 클래스 추가하기` 주석을 찾아 아래 내용을 추가합니다. 아래 코드는 앱 실행시 `IConfiguration` 인스턴스에서 `LlmProvider` 값을 찾아 그 값을 바탕으로 `IChatClient` 인스턴스를 생성하는 팩토리 메서드 패턴입니다.
   - `Ollama`: Ollama 연결 정보를 이용해서 `IChatClient` 인스턴스를 생성합니다.
   - `GitHubModels`: GitHub Models 연결 정보를 이용해서 `IChatClient` 인스턴스를 생성합니다.
   - `AzureOpenAI`: Azure OpenAI 연결 정보를 이용해서 `IChatClient` 인스턴스를 생성합니다.

    ```csharp
    // ChatClientFactory 클래스 추가하기
    public class ChatClientFactory
    {
        public static async Task<IChatClient> CreateChatClientAsync(IConfiguration config, IEnumerable<string> args)
        {
            var provider = config["LlmProvider"];

            // 커맨드라인 파라미터 확인 로직 추가하기

            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new InvalidOperationException("Missing configuration: LlmProvider");
            }
    
            IChatClient chatClient = provider switch
            {
                "Ollama" => await CreateOllamaChatClientAsync(config, provider),
                "GitHubModels" => await CreateGitHubModelsChatClientAsync(config, provider),
                "AzureOpenAI" => await CreateAzureOpenAIChatClientAsync(config, provider),
                _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
            };
    
            return chatClient;
        }
    
        // CreateOllamaChatClientAsync 메서드 추가하기

        // CreateGitHubModelsChatClientAsync 메서드 추가하기
    
        // CreateAzureOpenAIChatClientAsync 메서드 추가하기
    }
    ```

1. 같은 파일에서 `// 커맨드라인 파라미터 확인 로직 추가하기` 주석을 찾아 아래 내용을 추가합니다. 커맨드라인 파라미터의 `--provider` 값을 확인해서 기존 `appsettings.json` 파일의 값보다 우선적으로 적용할 수 있도록 합니다.

    ```csharp
    // 커맨드라인 파라미터 확인 로직 추가하기
    foreach (var arg in args)
    {
        var index = args.ToList().IndexOf(arg);
        switch (arg)
        {
            case "--provider":
                provider = args.ToList()[index + 1];
                break;
        }
    }
    ```

1. 같은 파일에서 `// CreateOllamaChatClientAsync 메서드 추가하기` 주석을 찾아 아래 내용을 추가합니다. Ollama 서버 연결 정보를 통해 `IChatClient` 인스턴스를 생성하는 코드입니다.

    ```csharp
    // CreateOllamaChatClientAsync 메서드 추가하기
    private static async Task<IChatClient> CreateOllamaChatClientAsync(IConfiguration config, string provider)
    {
        var ollama = config.GetSection("Ollama");
        var endpoint = ollama["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: Ollama:Endpoint");
        var model = ollama["Model"] ?? throw new InvalidOperationException("Missing configuration: Ollama:Model");

        Console.WriteLine();
        Console.WriteLine($"\tUsing {provider}: {model}");
        Console.WriteLine();

        var client = new OllamaApiClient(endpoint, model);

        var pulls = client.PullModelAsync(model);
        var status = default(string);
        await foreach (var pull in pulls)
        {
            if (status == pull?.Status)
            {
                continue;
            }

            Console.WriteLine($"Pulling model '{model}': {pull?.Status}");
            status = pull?.Status;
        }

        var chatClient = client as IChatClient;

        return chatClient;
    }
    ```

1. 같은 파일에서 `// CreateGitHubModelsChatClientAsync 메서드 추가하기` 주석을 찾아 아래 내용을 추가합니다. GitHub Models 연결 정보를 통해 `IChatClient` 인스턴스를 생성하는 코드입니다.

    ```csharp
    // CreateGitHubModelsChatClientAsync 메서드 추가하기
    private static async Task<IChatClient> CreateGitHubModelsChatClientAsync(IConfiguration config, string provider)
    {
        var github = config.GetSection("GitHub");
        var endpoint = github["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Endpoint");
        var token = github["Token"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Token");
        var model = github["Model"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Model");

        Console.WriteLine();
        Console.WriteLine($"\tUsing {provider}: {model}");
        Console.WriteLine();

        var credential = new ApiKeyCredential(token);
        var options = new OpenAIClientOptions()
        {
            Endpoint = new Uri(endpoint)
        };

        var client = new OpenAIClient(credential, options);
        var chatClient = client.GetChatClient(model)
                               .AsIChatClient();

        return await Task.FromResult(chatClient);
    }
    ```

1. 같은 파일에서 `// CreateAzureOpenAIChatClientAsync 메서드 추가하기` 주석을 찾아 아래 내용을 추가합니다. Azure OpenAI 연결 정보를 통해 `IChatClient` 인스턴스를 생성하는 코드입니다.

    ```csharp
    // CreateAzureOpenAIChatClientAsync 메서드 추가하기
    private static async Task<IChatClient> CreateAzureOpenAIChatClientAsync(IConfiguration config, string provider)
    {
        var azure = config.GetSection("Azure:OpenAI");
        var endpoint = azure["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:Endpoint");
        var apiKey = azure["ApiKey"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:ApiKey");
        var deploymentName = azure["DeploymentName"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:DeploymentName");

        Console.WriteLine();
        Console.WriteLine($"\tUsing {provider}: {deploymentName}");
        Console.WriteLine();

        var credential = new ApiKeyCredential(apiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{endpoint.TrimEnd('/')}/openai/v1/")
        };

        var client = new ResponsesClient(deploymentName, credential, options);
        var chatClient = client.AsIChatClient();

        return await Task.FromResult(chatClient);
    }
    ```

1. 같은 파일에서 `// IChatClient 인스턴스 생성하기` 주석을 찾아 아래와 같이 입력합니다. 앞서 작성한 팩토리 메서드 패턴을 이용해 GitHub Models 또는 Azure OpenAI 인스턴스를 `IChatClient` 타입으로 생성합니다.

    ```csharp
    // IChatClient 인스턴스 생성하기
    IChatClient? chatClient = await ChatClientFactory.CreateChatClientAsync(builder.Configuration, args);
    ```

1. 같은 파일에서 `// IChatClient 인스턴스 등록하기` 주석을 찾아 아래와 같이 입력합니다. 앞서 생성한 `IChatClient` 인스턴스를 의존성 개체로 등록합니다.

    ```csharp
    // IChatClient 인스턴스 등록하기
    builder.Services.AddChatClient(chatClient);
    ```

## 단일 에이전트 생성

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// Writer 에이전트 추가하기` 주석을 찾아 아래와 같이 입력합니다. 에이전트는 다양한 방법으로 추가할 수 있지만, 여기서는 가장 간단한 방법으로 에이전트 이름과 페르소나/지침을 입력합니다.

    ```csharp
    // Writer 에이전트 추가하기
    builder.AddAIAgent(
        name: "writer",
        instructions: "You write short stories (300 words or less) about the specified topic."
    );
    ```

1. 같은 파일에서 `// OpenAI 관련 응답 히스토리 핸들러 등록하기` 주석을 찾아 아래와 같이 입력합니다. 에이전트가 생성하는 응답과 대화 히스토리를 저장하는 서비스 인스턴스를 별도로 로직을 구현하지 않고 직접 의존성 개체로 등록합니다.

    ```csharp
    // OpenAI 관련 응답 히스토리 핸들러 등록하기
    builder.Services.AddOpenAIResponses();
    builder.Services.AddOpenAIConversations();
    ```

1. 같은 파일에서 `// OpenAI 관련 응답 히스토리 미들웨어 설정하기` 주석을 찾아 아래와 같이 입력합니다. 에이전트가 생성하는 응답과 대화 히스토리를 호출하는 엔드포인트를 미들웨어를 통해 각각 추가합니다.

    ```csharp
    // OpenAI 관련 응답 히스토리 미들웨어 설정하기
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();
    ```

## Dev UI 추가

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// Dev UI 미들웨어 설정하기` 주석을 찾아 아래와 같이 입력합니다. 로컬 개발환경에서 Dev UI 화면을 로딩할 수 있도록 `/devui` 엔드포인트를 미들웨어를 통해 추가합니다.

    ```csharp
    if (builder.Environment.IsDevelopment() == false)
    {
        app.UseHttpsRedirection();
    }
    // Dev UI 미들웨어 설정하기
    else
    {
        app.MapDevUI();
    }
    ```

1. 같은 파일에서 `// /devui 엔드포인트 자동 포워딩 설정하기` 주석을 찾아 아래와 같이 입력합니다. 웹사이트가 열리면 자동으로 `/devui` 엔드포인트로 포워딩해 줍니다.

    ```csharp
    // /devui 엔드포인트 자동 포워딩 설정하기
    app.MapGet("/", () => Results.Redirect("/devui"));
    ```

## 단일 에이전트 실행

### GitHub Models 활용

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 터미널에 현재 GitHub Models를 연결했다는 메시지가 나타나는 것을 확인합니다.

    ```text
    Using GitHubModels: openai/gpt-5-mini
    ```

1. 터미널에서 `CTRL`+`C`를 눌러 애플리케이션을 종료합니다.

1. 다시 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 단일 에이전트](./images/step-01-image-02.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Writer 에이전트 실행 결과](./images/step-01-image-03.png)

   > **NOTE**: 만약 gpt-5-mini 모델을 실행시킬 수 없을 때 에러가 생길 수 있습니다. 그럴 때는 `/MafWorkshop.Agent/appsettings.json` 파일을 열어 `GitHub` 섹션의 모델명을 `openai/gpt-5-mini`에서 `openai/gpt-4o-mini`로 바꿔보세요.
   >
   > ```jsonc
   > {
   >   "GitHub": {
   >     "Endpoint": "https://models.github.ai/inference",
   >     "Token": "{{GITHUB_PAT}}",
   >     "Model": "openai/gpt-4o-mini"
   >   }
   > }
   > ```

### Azure OpenAI 활용

> **Azure 구독이 있을 경우** 실행해 보세요.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/appsettings.json` 파일을 열어 아래와 같이 `LlmProvider` 값을 `AzureOpenAI`로 바꿔봅니다.

    ```jsonc
    {
      // 변경 전
      "LlmProvider": "GitHubModels",
    
      // 변경 후
      "LlmProvider": "AzureOpenAI",
    }
    ```

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 터미널에 현재 Azure OpenAI를 연결했다는 메시지가 나타나는 것을 확인합니다.

    ```text
    Using AzureOpenAI: gpt-5-mini
    ```

1. 터미널에서 `CTRL`+`C`를 눌러 애플리케이션을 종료합니다.

1. 다시 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 단일 에이전트](./images/step-01-image-02.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Writer 에이전트 실행 결과](./images/step-01-image-03.png)

1. 터미널에서 `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

### Ollama 활용

> **Ollama 서버를 통해 로컬 LLM**을 활용할 경우 실행해 보세요.

1. 터미널에서 Ollama 서버를 실행시킵니다.

    ```bash
    ollama serve
    ```

1. 새 터미널을 열고 워크샵 디렉토리로 이동합니다. 새 터미널에서는 `$REPOSITORY_ROOT` 값을 인식하지 못하므로 [리포지토리 루트 설정](#리포지토리-루트-설정) 섹션을 다시 다시 실행시켜야 합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 애플리케이션을 실행합니다. 이번에는 커맨드라인 파라미터를 이용해서 `LlmProvider` 값을 변경합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent -- --provider Ollama
    ```

1. 터미널에 현재 Ollama를 연결했다는 메시지가 나타나는 것을 확인합니다.

    ```text
    Using Ollama: granite4:350m
    ```

1. 터미널에서 `CTRL`+`C`를 눌러 애플리케이션을 종료합니다.

1. 다시 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent -- --provider Ollama
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 단일 에이전트](./images/step-01-image-02.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Writer 에이전트 실행 결과](./images/step-01-image-03.png)

1. 터미널에서 `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-01/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-01`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # zsh/bash
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-01/complete/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-01/complete/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

1. 워크샵 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 이전 [LLM 접근 권한 설정](#llm-접근-권한-설정)을 따라 LLM 접근 권한을 설정합니다.
1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. [단일 에이전트 실행](#단일-에이전트-실행) 섹션을 따라합니다.

---

축하합니다! Microsoft Agent Framework을 활용한 단일 에이전트 백엔드 개발이 끝났습니다. 이제 다음 단계로 이동하세요!

👈 [00: 개발 환경 설정](./00-setup.md) | [02: Microsoft Agent Framework에 프론트엔드 UI 연동하기](./02-ui-integration-with-maf.md) 👉
