using MafWorkshop.Agent;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

IChatClient? chatClient = await ChatClientFactory.CreateChatClientAsync(builder.Configuration, args);

builder.Services.AddChatClient(chatClient);

builder.AddAIAgent(
    name: "writer",
    instructions: "You write short stories (300 words or less) about the specified topic."
);

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// AG-UI 등록하기
builder.Services.AddAGUI();

var app = builder.Build();

app.MapOpenAIResponses();
app.MapOpenAIConversations();

// AG-UI 미들웨어 설정하기
app.MapAGUI(
    pattern: "ag-ui",
    aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("writer")
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
