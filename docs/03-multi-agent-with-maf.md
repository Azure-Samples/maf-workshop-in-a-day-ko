# 03: Microsoft Agent Framework 사용해서 다중 에이전트 개발하기

이 세션에서는 Microsoft Agent Framework를 사용해서 다중 에이전트 백엔드 애플리케이션을 개발합니다.

## 세션 목표

- Microsoft Agent Framework에 다중 에이전트를 설정하여 에이전트 워크플로우를 구성할 수 있습니다.
- Microsoft Agent Framework에서 동작하는 다중 에이전트의 흐름을 시각화할 수 있습니다.
- Microsoft Agent Framework에서 동작하는 다중 에이전트의 응답을 필터링할 수 있습니다.

## 아키텍처

이 세션이 끝나고 나면 아래와 같은 시스템이 만들어집니다.

![세션 아키텍처](./images/step-03-architecture.png)

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
└── step-03/
    └── start/
        ├── MafWorkshop.sln
        ├── MafWorkshop.Agent/
        │   ├── Properties/
        │   │   └── launchSettings.json
        │   ├── Program.cs
        │   ├── appsettings.json
        │   └── MafWorkshop.Agent.csproj
        └── MafWorkshop.WebUI/
            ├── Properties/
            │   └── launchSettings.json
            ├── Components/
            │   └── < Razor component files >
            ├── wwwroot/
            │   └── < HTML/CSS/JS files >
            ├── Program.cs
            ├── appsettings.json
            └── MafWorkshop.WebUI.csproj
```

> 프로젝트 소개:
>
> - `MafWorkshop.Agent`: 백엔드 에이전트 애플리케이션 프로젝트
> - `MafWorkshop.WebUI`: 프론트엔드 웹 UI 애플리케이션 프로젝트

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-02`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # zsh/bash
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-03/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-03/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

## 시작 프로젝트 빌드 및 실행

> 현재 시작 프로젝트는 단일 에이전트로만 구성해 둔 상태입니다.

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 백엔드 에이전트 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 다른 터미널을 열고 프론트엔드 UI 애플리케이션을 실행합니다. 새 터미널에서는 `$REPOSITORY_ROOT` 값을 인식하지 못하므로 [리포지토리 루트 설정](#리포지토리-루트-설정) 섹션을 다시 다시 실행시켜야 합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    dotnet watch run --project ./MafWorkshop.WebUI
    ```

1. 자동으로 웹 브라우저가 열리면서 아래와 같은 챗 UI 페이지가 나타나는지 확인합니다.

   ![웹 UI 페이지](./images/step-03-image-01.png)

   아무 문장이나 입력한 후 결과를 확인합니다.

   ![웹 UI 페이지 - 결과 확인](./images/step-03-image-02.png)

   > **NOTE**: 만약 GitHub Models를 사용하다 보면 사용량 초과(rate limit) 에러가 나타날 수 있습니다. 그러면 `/MafWorkshop.Agent/appsettings.json`의 `LlmProvider` 값을 `GitHubModels`에서 `AzureOpenAI`로 바꿔 진행합니다. 또는 커맨드라인 파라미터를 통해 `/MafWorkshop.Agent/appsettings.json` 파일을 직접 수정하지 않고 `LlmProvider` 값을 변경해 보세요.

1. 두 터미널에서 각각 `CTRL`+`C` 키를 눌러 모든 애플리케이션 실행을 종료합니다.

## 추가 에이전트 생성

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// AgentTools 클래스 추가하기` 주석을 찾아 아래와 같이 입력합니다. 추가 에이전트가 활용할 수 있는 도구를 구현합니다.

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

1. 같은 파일에서 `// Editor 에이전트 추가하기` 주석을 찾아 아래와 같이 입력합니다. 기존의 **Writer** 에이전트가 생성한 스토리를 이 **Editor** 에이전트가 받아 수정하는 역할을 부여합니다. 앞서 작성한 **Writer** 에이전트와 달리 이 **Editor** 에이전트는 에이전트 이름과 지침, 그리고 에이전트가 사용할 수 있는 도구인 `FormatStory`를 위해 별도의 Delegate 함수 형태로 만들어 봅니다.

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

## 다중 에이전트 워크플로우 생성

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.Agent/Program.cs` 파일을 열고 `// Publisher 워크플로우 추가하기` 주석을 찾아 아래와 같이 입력합니다. 아래 코드는 "**Writer**" 에이전트가 사용자의 입력을 받아 일차적으로 처리하고 그 결과물을 "**Editor**" 에이전트가 한 번 교정하는 Sequential 워크플로우입니다. 그리고 마지막에 `.AddAsAIAgent()` 메서드를 추가해서 이 워크플로우 역시 하나의 에이전트로 작동하게끔 구성합니다. 이 때 이 Publisher 워크플로우도 앞서 작성했던 Editor 에이전트와 마찬가지로 Delegate 함수 형태를 사용합니다.

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

1. 같은 파일에서 `// AG-UI 미들웨어 설정하기` 주석을 찾아 아래와 같이 변경합니다. 변경 전에는 AG-UI 엔드포인트가 **Writer** 에이전트를 가리켰지만, 변경 후에는 **Publisher** 워크플로우를 가리키게 됩니다.

   **변경전:**

    ```csharp
    // AG-UI 미들웨어 설정하기
    app.MapAGUI(
        pattern: "ag-ui",
        aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("writer")
    );
    ```

   **변경후:**

    ```csharp
    // AG-UI 미들웨어 설정하기
    app.MapAGUI(
        pattern: "ag-ui",
        aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("publisher")
    );
    ```

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

   ![DevUI 페이지 - 다중 에이전트](./images/step-03-image-03.png)

   Publisher 워크플로우를 선택합니다.

   ![DevUI 페이지 - Publisher 워크플로우](./images/step-03-image-04.png)

   메시지를 보내고 결과를 확인해 봅니다.

   ![Publisher 워크플로우에 메시지 보내기](./images/step-03-image-05.png)

   ![Publisher 워크플로우 실행 결과](./images/step-03-image-06.png)

1. `CTRL`+`C` 키를 눌러 애플리케이션 실행을 종료합니다.

## 전체 애플리케이션 빌드 및 실행 #1

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 백엔드 에이전트 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 다른 터미널을 열고 프론트엔드 UI 애플리케이션을 실행합니다. 새 터미널에서는 `$REPOSITORY_ROOT` 값을 인식하지 못하므로 [리포지토리 루트 설정](#리포지토리-루트-설정) 섹션을 다시 다시 실행시켜야 합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    dotnet watch run --project ./MafWorkshop.WebUI
    ```

1. 자동으로 웹 브라우저가 열리면서 아래와 같은 챗 UI 페이지가 나타나는지 확인합니다.

   ![웹 UI 페이지](./images/step-03-image-07.png)

   아무 문장이나 입력한 후 결과를 확인합니다.

   ![웹 UI 페이지 - 중복 응답 결과 확인](./images/step-03-image-08.png)

   같은 이야기를 두 번 반복합니다. 하나는 **Writer** 에이전트가 응답한 것, 다른 하나는 **Editor** 에이전트가 수정한 것. 따라서 처음 **Writer** 에이전트가 응답한 것은 걸러내야 합니다.

1. 두 터미널에서 각각 `CTRL`+`C` 키를 눌러 모든 애플리케이션 실행을 종료합니다.

## UI 컴포넌트 수정

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. `./MafWorkshop.WebUI/Components/Pages/Chat/Chat.razor` 파일을 열고 `// GetStreamingResponseAsync() 메서드 원본 삭제하기` 주석을 찾아 바로 아래에 있는 메서드를 삭제합니다. 이 메서드는 **Writer** 에이전트의 응답과 **Editor** 에이전트의 응답을 구분하지 못합니다.

    **삭제전:**

    ```csharp
    // GetStreamingResponseAsync() 메서드 원본 삭제하기
    private async Task GetStreamingResponseAsync(TextContent responseText, ChatMessage responseMessage, CancellationToken cancellationToken)
    {
        await foreach (var update in ChatClient.GetStreamingResponseAsync(messages.Skip(statefulMessageCount), chatOptions, cancellationToken))
        {
            await Task.Delay(50);

            messages.AddMessages(update, filter: c => c is not TextContent);
            responseText.Text += update.Text;
            chatOptions.ConversationId = update.ConversationId;
            ChatMessageItem.NotifyChanged(responseMessage);

            StateHasChanged();
        }
    }
    ```

    **삭제후:** 주석만 남아있습니다.

    ```csharp
    // GetStreamingResponseAsync() 메서드 원본 삭제하기
    ```

1. 같은 파일에서 `// GetStreamingResponseAsync() 메서드 리팩토링하기` 주석을 찾아 아래와 같이 입력합니다. 이제 이 메서드는 Tool을 통해 **Editor** 에이전트가 정리한 응답만 출력합니다.

    ```csharp
    // GetStreamingResponseAsync() 메서드 리팩토링하기
    private async Task GetStreamingResponseAsync(TextContent responseText, ChatMessage responseMessage, CancellationToken cancellationToken)
    {
        var deferAssistantTextUntilToolResponse = false;
        var toolResponseSeen = false;
        await foreach (var update in ChatClient.GetStreamingResponseAsync(messages.Skip(statefulMessageCount), chatOptions, cancellationToken))
        {
            await Task.Delay(50);

            messages.AddMessages(update, filter: c => c is not TextContent);

            if (deferAssistantTextUntilToolResponse == false && update.Role == ChatRole.Assistant && ContainsNonTextContent(update))
            {
                deferAssistantTextUntilToolResponse = true;
            }

            if (update.Role == ChatRole.Tool)
            {
                toolResponseSeen = true;
            }

            if ((deferAssistantTextUntilToolResponse == false || toolResponseSeen == true) && update.Role == ChatRole.Assistant)
            {
                responseText.Text += update.Text;
                ChatMessageItem.NotifyChanged(responseMessage);
            }

            chatOptions.ConversationId = update.ConversationId;
            StateHasChanged();
        }

        bool ContainsNonTextContent(ChatResponseUpdate update)
        {
            if (update.Contents.Any() == false)
            {
                return true;
            }

            if (update.Contents.Any(c => c is not TextContent) == true)
            {
                return true;
            }

            return false;
        }
    }
    ```

## 전체 애플리케이션 빌드 및 실행 #2

1. 워크샵 디렉토리에 있는지 다시 한 번 확인합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 전체 프로젝트를 빌드합니다.

    ```bash
    dotnet restore && dotnet build
    ```

1. 백엔드 에이전트 애플리케이션을 실행합니다.

    ```bash
    dotnet run --project ./MafWorkshop.Agent
    ```

1. 다른 터미널을 열고 프론트엔드 UI 애플리케이션을 실행합니다. 새 터미널에서는 `$REPOSITORY_ROOT` 값을 인식하지 못하므로 [리포지토리 루트 설정](#리포지토리-루트-설정) 섹션을 다시 다시 실행시켜야 합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    dotnet watch run --project ./MafWorkshop.WebUI
    ```

1. 자동으로 웹 브라우저가 열리면서 아래와 같은 챗 UI 페이지가 나타나는지 확인합니다.

   ![웹 UI 페이지](./images/step-03-image-07.png)

   아무 문장이나 입력한 후 결과를 확인합니다.

   ![웹 UI 페이지 - 최종 응답 결과 확인](./images/step-03-image-09.png)

   이제 **Editor** 에이전트가 수정한 응답만 출력합니다.

1. 두 터미널에서 각각 `CTRL`+`C` 키를 눌러 모든 애플리케이션 실행을 종료합니다.

## 완성본 결과 확인

이 세션의 완성본은 `$REPOSITORY_ROOT/save-points/step-03/complete`에서 확인할 수 있습니다.

1. 앞서 실습한 `workshop` 디렉토리가 있다면 삭제하거나 다른 이름으로 바꿔주세요. 예) `workshop-step-03`
1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # zsh/bash
    mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-03/complete/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-03/complete/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```

1. 워크샵 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. [전체 애플리케이션 빌드 및 실행 #2](#전체-애플리케이션-빌드-및-실행-2) 섹션을 따라합니다.

---

축하합니다! Microsoft Agent Framework을 활용한 에이전트 백엔드 개발이 끝났습니다. 이제 다음 단계로 이동하세요!

👈 [02: Microsoft Agent Framework에 프론트엔드 UI 연동하기](./02-ui-integration-with-maf.md) | [04: Aspire로 프론트엔드 웹 UI와 백엔드 에이전트 오케스트레이션하기](./04-aspire-orchestration.md) 👉
