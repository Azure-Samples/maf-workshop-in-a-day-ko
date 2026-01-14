using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// HttpClientFactory 등록하기
builder.Services.AddHttpClient("mcptodo", client =>
{
    client.BaseAddress = new Uri("https+http://mcptodo");
});

// MCP 클라이언트 등록하기
builder.Services.AddSingleton<McpClient>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                       .CreateClient("mcptodo");

    var clientTransportOptions = new HttpClientTransportOptions()
    {
        Endpoint = new Uri($"{httpClient.BaseAddress!.ToString().Replace("+http", string.Empty).TrimEnd('/')}/mcp")
    };
    var clientTransport = new HttpClientTransport(clientTransportOptions, httpClient, loggerFactory);

    var clientOptions = new McpClientOptions()
    {
        ClientInfo = new Implementation()
        {
            Name = "MCP Todo Client",
            Version = "1.0.0",
        }
    };

    return McpClient.CreateAsync(clientTransport, clientOptions, loggerFactory).GetAwaiter().GetResult();
});

builder.AddOpenAIClient("chat")
       .AddChatClient();

// Manager 에이전트 추가하기
builder.AddAIAgent(
    name: "manager",
    createAgentDelegate: (sp, key) =>
    {
        var chatClient = sp.GetRequiredService<IChatClient>();
        var mcpClient = sp.GetRequiredService<McpClient>();
        var tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
        var agent = new ChatClientAgent(
            chatClient: chatClient,
            name: key,
            instructions: """
                You manage my todo list items.
                When I ask for the list, provide all the items in a numbered format with their complete status.
                When I give you a new todo item, add it to the list.
                When I give you an updated todo item, update it in the list.
                When I ask you to mark an item as done, mark it as completed.
                When I ask you to remove an item, delete it from the list.
                When I ask you to clear the list, remove all items.
                """,
            tools: [.. tools]
        );

        return agent;
    }
);

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

builder.Services.AddAGUI();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenAIResponses();
app.MapOpenAIConversations();

// AG-UI 미들웨어 설정하기
app.MapAGUI(
    pattern: "ag-ui",
    aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("manager")
);

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
else
{
    app.MapDevUI();
}

await app.RunAsync();
