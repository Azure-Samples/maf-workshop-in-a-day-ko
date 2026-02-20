var builder = DistributedApplication.CreateBuilder(args);

// MCP Todo 서버 프로젝트 추가하기
var mcptodo = builder.AddProject<Projects.MafWorkshop_McpTodo>("mcptodo")
                     .WithExternalHttpEndpoints();

// 백엔드 에이전트 프로젝트 수정하기
var agent = builder.AddProject<Projects.MafWorkshop_Agent>("agent")
                   .WithExternalHttpEndpoints()
                   .WithLlmReference(builder.Configuration, args)
                   .WithReference(mcptodo)
                   .WaitFor(mcptodo);
// var agent = builder.AddProject<Projects.MafWorkshop_Agent>("agent")
//                    .WithExternalHttpEndpoints()
//                    .WithLlmReference(builder.Configuration);

var webUI = builder.AddProject<Projects.MafWorkshop_WebUI>("webui")
                   .WithExternalHttpEndpoints()
                   .WithReference(agent)
                   .WaitFor(agent);

await builder.Build().RunAsync();
