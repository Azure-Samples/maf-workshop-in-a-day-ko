using System.ClientModel;

using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

using OllamaSharp;

using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

var builder = WebApplication.CreateBuilder(args);

// IChatClient 인스턴스 생성하기

// IChatClient 인스턴스 등록하기

// Writer 에이전트 추가하기

// OpenAI 관련 응답 히스토리 핸들러 등록하기

var app = builder.Build();

// OpenAI 관련 응답 히스토리 미들웨어 설정하기

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
// DevUI 미들웨어 설정하기

// /devui 엔드포인트 자동 포워딩 설정하기

await app.RunAsync();

// ChatClientFactory 클래스 추가하기
