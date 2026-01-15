using MafWorkshop.WebUI;
using MafWorkshop.WebUI.Components;

using Microsoft.Agents.AI.AGUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

// HttpClientFactory 등록하기

// AG-UI 연동 IChatClient 인스턴스 등록하기
builder.Services.AddChatClient(new FakeChatClient());

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
