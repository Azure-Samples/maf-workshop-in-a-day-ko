using Microsoft.Extensions.Configuration;

public static class LlmResourceFactory
{
    public static IResourceBuilder<ProjectResource> WithLlmReference(this IResourceBuilder<ProjectResource> source, IConfiguration config, IEnumerable<string> args)
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

        source = provider switch
        {
            "GitHubModels" => source.AddGitHubModelsResource(config, provider),
            "AzureOpenAI" => source.AddAzureOpenAIResource(config, provider),
            _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
        };

        return source;
    }

    private static IResourceBuilder<ProjectResource> AddGitHubModelsResource(this IResourceBuilder<ProjectResource> source, IConfiguration config, string provider)
    {
        var github = config.GetSection("GitHub");
        var endpoint = github["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Endpoint");
        var token = github["Token"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Token");
        var model = github["Model"] ?? throw new InvalidOperationException("Missing configuration: GitHub:Model");

        Console.WriteLine();
        Console.WriteLine($"\tUsing {provider}: {model}");
        Console.WriteLine();

        var apiKey = source.ApplicationBuilder
                           .AddParameter(name: "apiKey", value: token, secret: true);
        var chat = source.ApplicationBuilder
                         .AddGitHubModel(name: "chat", model: model)
                         .WithApiKey(apiKey);

        return source.WithReference(chat)
                     .WaitFor(chat);
    }

    private static IResourceBuilder<ProjectResource> AddAzureOpenAIResource(this IResourceBuilder<ProjectResource> source, IConfiguration config, string provider)
    {
        var azure = config.GetSection("Azure:OpenAI");
        var endpoint = azure["Endpoint"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:Endpoint");
        var accessKey = azure["ApiKey"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:ApiKey");
        var deploymentName = azure["DeploymentName"] ?? throw new InvalidOperationException("Missing configuration: Azure:OpenAI:DeploymentName");

        Console.WriteLine();
        Console.WriteLine($"\tUsing {provider}: {deploymentName}");
        Console.WriteLine();

        var apiKey = source.ApplicationBuilder
                           .AddParameter(name: "apiKey", value: accessKey, secret: true);
        var chat = source.ApplicationBuilder
                         .AddOpenAI("openai")
                         .WithEndpoint($"{endpoint.TrimEnd('/')}/openai/v1/")
                         .WithApiKey(apiKey)
                         .AddModel(name: "chat", model: deploymentName);

        return source.WithReference(chat)
                     .WaitFor(chat);
    }
}
