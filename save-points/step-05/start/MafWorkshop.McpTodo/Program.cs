using System.ComponentModel;
using System.Reflection;

using MafWorkshop.McpTodo;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

var connection = new SqliteConnection("Filename=:memory:");
connection.Open();

builder.Services.AddSingleton(connection);

builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(connection));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// MCP 서버 추가하기

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

// MCP 엔드포인트 미들웨어 추가하기

await app.RunAsync();

// Todo Tool 추가하기
