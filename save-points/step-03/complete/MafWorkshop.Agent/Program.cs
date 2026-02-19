using System.ComponentModel;

using MafWorkshop.Agent;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

IChatClient? chatClient = await ChatClientFactory.CreateChatClientAsync(builder.Configuration, args);

builder.Services.AddChatClient(chatClient);

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

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

builder.Services.AddAGUI();

var app = builder.Build();

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
else
{
    app.MapDevUI();
}

app.MapGet("/", () => Results.Redirect("/devui"));

await app.RunAsync();

// AgentTools 클래스 추가하기
public class AgentTools
{
    [Description("Formats the story for publication, revealing its title.")]
    public static string FormatStory(string title, string story) => $"""
        **Title**: {title}

        {story}
        """;
}