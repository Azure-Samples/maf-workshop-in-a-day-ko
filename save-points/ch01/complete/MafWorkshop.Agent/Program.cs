using MafWorkshop.Agent.Factory;
using MafWorkshop.Agent.Tools;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

IChatClient? chatClient = default;

// Create IChateClient instance based on configuration
chatClient = ChatClientFactory.CreateChatClient(builder.Configuration);

builder.Services.AddChatClient(chatClient);

builder.AddAIAgent("writer", "You write short stories (300 words or less) about the specified topic.");

// Add an editor agent that uses a tool to format stories for publishing
builder.AddAIAgent("editor", (sp, key) => new ChatClientAgent(
    chatClient,
    name: key,
    instructions: "You edit short stories to improve grammar and style, ensuring the stories are less than 300 words. Once finished editing, you select a title and format the story for publishing.",
    tools: [AIFunctionFactory.Create(AgentTool.FormatStory)]
));

// Add a publisher workflow that uses the writer and editor agents sequentially
builder.AddWorkflow("publisher", (sp, key) => AgentWorkflowBuilder.BuildSequential(
    workflowName: key,
    agents:
    [
        sp.GetRequiredKeyedService<AIAgent>("writer"),
        sp.GetRequiredKeyedService<AIAgent>("editor")
    ]
)).AddAsAIAgent();

// Register services for OpenAI responses and conversations (also required for DevUI)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// Map endpoints for OpenAI responses and conversations (also required for DevUI)
app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment() == true)
{
    // Map DevUI endpoint to /devui
    app.MapDevUI();
}
else
{
    app.UseHttpsRedirection();
}

await app.RunAsync();
