using MafWorkshop.WebUI.Components;

using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

// HttpClientFactory 등록하기

// AG-UI 연동 IChatClient 인스턴스 등록하기
builder.Services.AddChatClient(new FakeChatClient());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() == false)
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.UseStaticFiles();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

await app.RunAsync();

public class FakeChatClient : IChatClient
{
    private static readonly string[] responses =
    [
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        "Ut consequat risus eu laoreet mattis.",
        "Nam et velit in neque fermentum placerat ac consequat ligula.",
        "Ut vel neque ac diam ultricies venenatis sed eu augue.",
        "Vivamus eu lorem eleifend, placerat augue lobortis, consectetur eros.",
        "Maecenas mattis nulla quis placerat elementum.",
        "Fusce nec magna et quam convallis venenatis sit amet quis enim.",
        "Nunc quis metus non libero fringilla porta dignissim sit amet ante.",
        "Pellentesque in orci non libero condimentum vestibulum.",
        "Proin tincidunt erat interdum, vestibulum magna ac, tincidunt mi.",
        "Vestibulum fermentum risus vel magna maximus lacinia.",
        "Ut eget turpis eget ipsum elementum rhoncus et at quam.",
    ];

    public void Dispose()
    {
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, string.Join(" ", responses)));

        return Task.FromResult(response);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var updates = responses.Select(text => new ChatResponseUpdate(ChatRole.Assistant, text)).ToAsyncEnumerable();

        return updates;
    }
}