using System.ClientModel;

using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

using OllamaSharp;

using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

var builder = WebApplication.CreateBuilder(args);

// IChatClient 인스턴스 생성하기
IChatClient? chatClient = await ChatClientFactory.CreateChatClientAsync(builder.Configuration, args);

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
// DevUI 미들웨어 설정하기
else
{
    app.MapDevUI();
}

// /devui 엔드포인트 자동 포워딩 설정하기
app.MapGet("/", () => Results.Redirect("/devui"));

await app.RunAsync();

// ChatClientFactory 클래스 추가하기
public class ChatClientFactory
{
    public static async Task<IChatClient> CreateChatClientAsync(IConfiguration config, IEnumerable<string> args)
    {
        var provider = config["LlmProvider"];

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
}
