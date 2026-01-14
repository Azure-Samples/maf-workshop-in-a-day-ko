using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// 백엔드 에이전트 프로젝트 추가하기

// 프론트엔드 웹 UI 프로젝트 추가하기

await builder.Build().RunAsync();

// LlmResourceFactory 클래스 추가하기
