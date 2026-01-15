using System.ClientModel;

using Microsoft.Extensions.AI;

using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace MafWorkshop.Agent;

public class ChatClientFactory
{
    public static IChatClient CreateChatClient(IConfiguration config)
    {
        var provider = config["LlmProvider"] ?? throw new InvalidOperationException("Missing configuration: LlmProvider");
        IChatClient chatClient = provider switch
        {
            "GitHubModels" => CreateGitHubModelsChatClient(config),
            "AzureOpenAI" => CreateAzureOpenAIChatClient(config),
            _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
        };

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
}
