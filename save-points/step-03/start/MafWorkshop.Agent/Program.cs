using System.ClientModel;
using System.ComponentModel;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

var builder = WebApplication.CreateBuilder(args);

// IChatClient 인스턴스 생성하기
IChatClient? chatClient = ChatClientFactory.CreateChatClient(builder.Configuration);

// IChatClient 인스턴스 등록하기
builder.Services.AddChatClient(chatClient);

// Writer 에이전트 추가하기
builder.AddAIAgent(
    name: "writer",
    instructions: "You write short stories (300 words or less) about the specified topic."
);

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

// OpenAI 관련 응답 히스토리 핸들러 등록하기
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// AG-UI 등록하기
builder.Services.AddAGUI();

var app = builder.Build();

// OpenAI 관련 응답 히스토리 미들웨어 설정하기
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// AG-UI 미들웨어 설정하기
app.MapAGUI(
    pattern: "ag-ui",
    aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("publisher")
);

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
// DevUI 미들웨어 설정하기
else
{
    app.MapDevUI();
}

await app.RunAsync();

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

        return chatClient;
    }

    private static IChatClient CreateGitHubModelsChatClient(IConfiguration config)
    {
        var provider = config["LlmProvider"];

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

        return chatClient;
    }
}

// AgentTools 클래스 추가하기
public class AgentTools
{
    [Description("Formats the story for publication, revealing its title.")]
    public static string FormatStory(string title, string story) => $"""
        **Title**: {title}

        {story}
        """;
}