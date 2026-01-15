using MafWorkshop.WebUI;
using MafWorkshop.WebUI.Components;

using Microsoft.Agents.AI.AGUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

// HttpClientFactory 등록하기
builder.Services.AddHttpClient("agent", client =>
{
    var endpoint = builder.Environment.IsDevelopment() == true
        ? builder.Configuration["AgentEndpoints:Http"]
        : builder.Configuration["AgentEndpoints:Https"];
    client.BaseAddress = new Uri(endpoint!);
});

// AG-UI 연동 IChatClient 인스턴스 등록하기
builder.Services.AddChatClient(sp => new AGUIChatClient(
    httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("agent"),
    endpoint: "ag-ui")
);
// builder.Services.AddChatClient(new FakeChatClient());

var app = builder.Build();

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
