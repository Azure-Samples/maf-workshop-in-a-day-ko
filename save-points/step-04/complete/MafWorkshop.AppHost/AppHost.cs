using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// 백엔드 에이전트 프로젝트 추가하기
var agent = builder.AddProject<Projects.MafWorkshop_Agent>("agent")
                   .WithExternalHttpEndpoints()
                   .WithLlmReference(builder.Configuration);

// 프론트엔드 웹 UI 프로젝트 추가하기
var webUI = builder.AddProject<Projects.MafWorkshop_WebUI>("webui")
                   .WithExternalHttpEndpoints()
                   .WithReference(agent)
                   .WaitFor(agent);

await builder.Build().RunAsync();

// LlmResourceFactory 클래스 추가하기
public static class LlmResourceFactory
{
    public static IResourceBuilder<ProjectResource> WithLlmReference(this IResourceBuilder<ProjectResource> source, IConfiguration config)
    {
        var provider = config["LlmProvider"] ?? throw new InvalidOperationException("Missing configuration: LlmProvider");
        source = provider switch
        {
            "GitHubModels" => AddGitHubModelsResource(source, config),
            "AzureOpenAI" => AddAzureOpenAIResource(source, config),
            _ => throw new NotSupportedException($"The specified LLM provider '{provider}' is not supported.")
        };

        return source;
    }

    private static IResourceBuilder<ProjectResource> AddGitHubModelsResource(IResourceBuilder<ProjectResource> source, IConfiguration config)
    {
        var provider = config["LlmProvider"];

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

    private static IResourceBuilder<ProjectResource> AddAzureOpenAIResource(IResourceBuilder<ProjectResource> source, IConfiguration config)
    {
        var provider = config["LlmProvider"];

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
