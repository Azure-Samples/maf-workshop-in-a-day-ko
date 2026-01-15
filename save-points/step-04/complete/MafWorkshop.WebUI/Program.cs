using MafWorkshop.WebUI.Components;

using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Observability 및 Traceability를 위한 Service Defaults 추가하기
builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

// HttpClientFactory 등록하기
builder.Services.AddHttpClient("agent", client =>
{
    client.BaseAddress = new Uri("https+http://agent");
});
// builder.Services.AddHttpClient("agent", client =>
// {
//     var endpoint = builder.Environment.IsDevelopment() == true
//         ? builder.Configuration["AgentEndpoints:Http"]
//         : builder.Configuration["AgentEndpoints:Https"];
//     client.BaseAddress = new Uri(endpoint!);
// });

builder.Services.AddChatClient(sp => new AGUIChatClient(
    httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("agent"),
    endpoint: "ag-ui")
);

var app = builder.Build();

// Observability 및 Traceability를 위한 미들웨어 설정하기
app.MapDefaultEndpoints();

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
