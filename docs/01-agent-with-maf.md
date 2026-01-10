# 01: Microsoft Agent Framework 사용해서 에이전트 개발하기

이 세션에서는 Microsoft Agent Framework를 사용해서 백엔드 에이전트를 개발합니다.

## 세션 목표

- Microsoft Agent Framework에 다양한 LLM을 연결할 수 있습니다.
- Microsoft Agent Framework에 단일 에이전트를 붙일 수 있습니다.
- Microsoft Agent Framework에 다중 에이전트를 붙여 에이전트 워크플로우를 구성할 수 있습니다.
- Microsoft Agent Framework에서 동작하는 에이전트의 흐름을 시각화할 수 있습니다.

## 사전 준비 사항

이전 [00: 개발 환경 설정하기](./00-setup.md)에서 개발 환경을 모두 설정한 상태라고 가정합니다.

## 리포지토리 루트 설정

1. 아래 명령어를 실행시켜 `$REPOSITORY_ROOT` 환경 변수를 설정합니다.

    ```bash
    # Bash/Zsh
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

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
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
    # Azure OpenAI
    dotnet user-secrets --project ./MafWorkshop.Agent set Azure:OpenAI:Endpoint $endpoint
    dotnet user-secrets --project ./MafWorkshop.Agent set Azure:OpenAI:ApiKey $apiKey
    
    # GitHub Models
    dotnet user-secrets --project ./MafWorkshop.Agent set GitHub:Token $githubToken
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

1. `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## LLM 연결

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 맨 아래로 이동해서 `// ChatClientFactory 클래스 추가하기` 주석을 찾아 아래 내용을 추가합니다.

   > 아래 코드는 `IConfiguration` 인스턴스에서 `LlmProvider` 값을 찾아 그 값이 `AzureOpenAI`이면 Azure OpenAI 연결 정보를 이용해서 `IChatClient` 인스턴스를 생성하고, `GitHubModels`이면 GitHub Models 연결 정보를 이용해서 `IChatClient` 인스턴스를 생성하는 팩토리 메서드 패턴입니다.

    ```csharp
    // ChatClientFactory 클래스 추가하기
    public class ChatClientFactory
    {
        public static IChatClient CreateChatClient(IConfiguration config)
        {
            var provider = config["LlmProvider"] ?? throw new InvalidOperationException("Missing configuration: LlmProvider");
            IChatClient chatClient = provider switch
            {
                "AzureOpenAI" => CreateAzureOpenAIChatClient(config),
                "GitHubModels" => CreateGitHubModelsChatClient(config),
                _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
            };
    
            return chatClient;
        }
    
        private static IChatClient CreateAzureOpenAIChatClient(IConfiguration config)
        {
            var provider = config["LlmProvider"];
    
            var azure = config.GetSection("Azure:OpenAI");
            var endpoint = azure["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:Endpoint");
            var apiKey = azure["ApiKey"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:ApiKey");
            var deploymentName = azure["DeploymentName"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:DeploymentName");
    
            Console.WriteLine($"Using {provider}: {deploymentName}");
    
            var credential = new ApiKeyCredential(apiKey);
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri($"{endpoint.TrimEnd('/')}/openai/v1/")
            };
    
            var client = new ResponsesClient(deploymentName, credential, options);
            var chatClient = client.AsIChatClient();
    
            return chatClient;
        }
    
        private static IChatClient CreateGitHubModelsChatClient(IConfiguration config)
        {
            var provider = config["LlmProvider"];
    
            var github = config.GetSection("GitHub");
            var endpoint = github["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Endpoint");
            var token = github["Token"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Token");
            var model = github["Model"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Model");
    
            Console.WriteLine($"Using {provider}: {model}");
    
            var credential = new ApiKeyCredential(token);
            var options = new OpenAIClientOptions()
            {
                Endpoint = new Uri(endpoint)
            };
    
            var client = new OpenAIClient(credential, options);
            var chatClient = client.GetChatClient(model)
                                   .AsIChatClient();
    
            return chatClient;
        }
    }
    ```

1. 같은 파일에서 `// IChatClient 인스턴스 생성하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // IChatClient 인스턴스 생성하기
    IChatClient? chatClient = ChatClientFactory.CreateChatClient(builder.Configuration);
    ```

1. 같은 파일에서 `// IChatClient 인스턴스 등록하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // IChatClient 인스턴스 등록하기
    builder.Services.AddChatClient(chatClient);
    ```

## 단일 에이전트 생성

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// Writer 에이전트 추가하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // Writer 에이전트 추가하기
    builder.AddAIAgent(
        name: "writer",
        instructions: "You write short stories (300 words or less) about the specified topic."
    );
    ```

   > 에이전트는 다양한 방법으로 추가할 수 있지만, 여기서는 가장 간단한 방법으로 에이전트 이름과 페르소나/지침을 입력합니다.

1. 같은 파일에서 `// OpenAI 관련 응답 히스토리 핸들러 등록하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // OpenAI 관련 응답 히스토리 핸들러 등록하기
    builder.Services.AddOpenAIResponses();
    builder.Services.AddOpenAIConversations();
    ```

1. 같은 파일에서 `// OpenAI 관련 응답 히스토리 미들웨어 설정하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // OpenAI 관련 응답 히스토리 미들웨어 설정하기
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();
    ```

## 에이전트 UI 추가

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// DevUI 미들웨어 설정하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    if (builder.Environment.IsDevelopment() == false)
    {
        app.UseHttpsRedirection();
    }
    // DevUI 미들웨어 설정하기
    else
    {
        app.MapDevUI();
    }
    ```

1. 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 터미널에 현재 어떤 LLM을 연결했는지 메시지가 나타나는 것을 확인합니다. 기본값은 GitHub Models 입니다.

    ```text
    Using GitHubModels: openai/gpt-5-mini
    ```

   `CTRL`+`C`를 눌러 애플리케이션을 종료하고 `./MafWorkshop.Agent/appsettings.json` 파일을 열어 아래와 같이 `LlmProvider` 값을 `AzureOpenAI`로 바꿔봅니다.

    ```jsonc
    {
      // 변경 전
      "LlmProvider": "GitHubModels",
    
      // 변경 후
      "LlmProvider": "AzureOpenAI",
    }
    ```

   이후 다시 앱을 실행시켜서 이번에는 `Using AzureOpenAI: gpt-5-mini` 메시지가 터미널 화면에 나타나는 것을 확인합니다. 이후 앱을 종료합니다.

## 단일 에이전트 실행

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 다시 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 단일 에이전트](./images/step-01-image-02.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Writer 에이전트 실행 결과](./images/step-01-image-03.png)

1. `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## 다중 에이전트 워크플로우

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// AgentTools 클래스 추가하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // AgentTools 클래스 추가하기
    public class AgentTools
    {
        [Description("Formats the story for publication, revealing its title.")]
        public static string FormatStory(string title, string story) => $"""
            **Title**: {title}
    
            {story}
            """;
    }
    ```

1. 같은 파일에서 `// Editor 에이전트 추가하기` 주석을 찾아 아래와 같이 입력합니다.

    ```csharp
    // Editor 에이전트 추가하기
    builder.AddAIAgent(
        name: "editor",
        createAgentDelegate: (sp, key) => new ChatClientAgent(
            chatClient: sp.GetRequiredService<IChatClient>(),
            name: key,
            instructions: """
                You edit short stories to improve grammar and style, ensuring the stories are less than 300 words. Once finished editing, you select a title and format the story for publishing.
                """,
            tools: [ AIFunctionFactory.Create(AgentTools.FormatStory) ]
        )
    );
    ```

   > 이 에이전트는 에이전트 이름과 지침 그리고 에이전트가 사용할 수 있는 도구를 추가하기 위해 별도의 delegate 함수를 사용했습니다.

1. 같은 파일에서 `// Publisher 워크플로우 추가하기` 주석을 찾아 아래와 같이 입력합니다. 아래 코드는 "**Writer**" 에이전트가 사용자의 입력을 받아 일차적으로 처리하고 그 결과물을 "**Editor**" 에이전트가 한 번 교정하는 Sequential 워크플로우입니다. 그리고 마지막에 `.AddAsAIAgent()` 메서드를 추가해서 이 워크플로우 역시 하나의 에이전트로 작동하게끔 구성했습니다.

    ```csharp
    // Publisher 워크플로우 추가하기
    builder.AddWorkflow(
        name: "publisher",
        createWorkflowDelegate: (sp, key) => AgentWorkflowBuilder.BuildSequential(
            workflowName: key,
            agents:
            [
                sp.GetRequiredKeyedService<AIAgent>("writer"),
                sp.GetRequiredKeyedService<AIAgent>("editor")
            ]
        )
    ).AddAsAIAgent();
    ```

   > Publisher 워크플로우도 에이전트 선언과 마찬가지로 이름과 delegate 함수를 사용했습니다.

## 다중 에이전트 워크플로우 실행

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 다시 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 다중 에이전트](./images/step-01-image-04.png)

   Publisher 워크플로우를 선택합니다.

   ![DevUI 페이지 - Publisher 워크플로우](./images/step-01-image-05.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Publisher 워크플로우에 메시지 보내기](./images/step-01-image-06.png)

   ![Publisher 워크플로우 실행 결과](./images/step-01-image-07.png)

1. `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-01/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-01`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
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
1. 백엔드 에이전트 애플리케이션을 실행합니다.

    ```bash
    dotnet watch run --project ./MafWorkshop.Agent
    ```

1. 자동으로 웹 브라우저가 열리면서 DevUI 페이지가 나타나는지 확인합니다.

   ![DevUI 페이지 - 다중 에이전트](./images/step-01-image-04.png)

   Publisher 워크플로우를 선택합니다.

   ![DevUI 페이지 - Publisher 워크플로우](./images/step-01-image-05.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Publisher 워크플로우에 메시지 보내기](./images/step-01-image-06.png)

   ![Publisher 워크플로우 실행 결과](./images/step-01-image-07.png)

1. `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

---

축하합니다! Microsoft Agent Framework을 활용한 에이전트 백엔드 개발이 끝났습니다. 이제 다음 단계로 이동하세요!

👈 [00: 개발 환경 설정](./00-setup.md) | [02: Microsoft Agent Framework에 웹 UI 연동하기](./02-web-ui-integration-with-maf.md) 👉
