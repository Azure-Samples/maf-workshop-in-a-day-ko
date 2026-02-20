var builder = DistributedApplication.CreateBuilder(args);

// MCP Todo 서버 프로젝트 추가하기

// 백엔드 에이전트 프로젝트 수정하기
var agent = builder.AddProject<Projects.MafWorkshop_Agent>("agent")
                   .WithExternalHttpEndpoints()
                   .WithLlmReference(builder.Configuration, args);

var webUI = builder.AddProject<Projects.MafWorkshop_WebUI>("webui")
                   .WithExternalHttpEndpoints()
                   .WithReference(agent)
                   .WaitFor(agent);

await builder.Build().RunAsync();
