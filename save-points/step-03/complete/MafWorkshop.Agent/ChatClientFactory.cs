using System.ClientModel;

using Microsoft.Extensions.AI;

using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace MafWorkshop.Agent;

public class ChatClientFactory
{
    public static async Task<IChatClient> CreateChatClientAsync(IConfiguration config, IEnumerable<string> args)
    {
        var provider = config["LlmProvider"];
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
            "GitHubModels" => await CreateGitHubModelsChatClientAsync(config, provider),
            "AzureOpenAI" => await CreateAzureOpenAIChatClientAsync(config, provider),
            _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
        };

        return chatClient;
    }

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
