using System.ClientModel;
using System.ComponentModel;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
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

// OpenAI 관련 응답 히스토리 핸들러 등록하기
    builder.Services.AddOpenAIResponses();
    builder.Services.AddOpenAIConversations();

var app = builder.Build();

// OpenAI 관련 응답 히스토리 미들웨어 설정하기
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
// Dev UI 미들웨어 설정하기
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
