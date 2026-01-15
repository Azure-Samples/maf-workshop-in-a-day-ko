using MafWorkshop.Agent;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Observability 및 Traceability를 위한 Service Defaults 추가하기

// IChatClient 인스턴스 생성하기
IChatClient? chatClient = ChatClientFactory.CreateChatClient(builder.Configuration);

// IChatClient 인스턴스 등록하기
builder.Services.AddChatClient(chatClient);

builder.AddAIAgent(
    name: "writer",
    instructions: "You write short stories (300 words or less) about the specified topic."
);

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

// Observability 및 Traceability를 위한 미들웨어 설정하기

app.MapOpenAIResponses();
app.MapOpenAIConversations();

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

await app.RunAsync();
