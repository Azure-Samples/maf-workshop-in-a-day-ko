using System.Reflection;

using MafWorkshop.McpTodo;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = new SqliteConnection("Filename=:memory:");
connection.Open();

builder.Services.AddSingleton(connection);

builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(connection));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddMcpServer()
                .WithHttpTransport(o => o.Stateless = true)
                .WithToolsFromAssembly(Assembly.GetEntryAssembly());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}

app.MapMcp("/mcp");

await app.RunAsync();
