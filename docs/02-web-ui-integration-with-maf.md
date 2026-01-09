# 02: Microsoft Agent Framework에 웹 UI 연동하기

이 세션에서는 Microsoft Agent Framework로 만들어진 백엔드 에이전트에 웹 UI를 연동합니다.

## 세션 목표

- Microsoft Agent Framework에 AGi-UI 프로토콜을 이용해서 프론트엔드 UI를 연결할 수 있습니다.

## 사전 준비 사항

이전 [00: 개발 환경 설정](./00-setup.md)에서 개발 환경을 모두 설정한 상태라고 가정합니다.

## 리포지토리 루트 설정

1. 아래 명령어를 실행시켜 `$REPOSITORY_ROOT` 환경 변수를 설정합니다.

    ```bash
    # Bash/Zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    ```

## 시작 프로젝트 복사

이 워크샵을 위해 필요한 시작 프로젝트를 준비해 뒀습니다. 시작 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
save-points/
└── step-02/
    └── start/
        ├── MafWorkshop.sln
        └── MafWorkshop.Agent/
            ├── Properties/
            │   └── launchSettings.json
            ├── Program.cs
            ├── appsettings.json
            └── MafWorkshop.Agent.csproj
```

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 시작 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    rm -rf $REPOSITORY_ROOT/workshop && \
        mkdir -p $REPOSITORY_ROOT/workshop && \
        cp -a $REPOSITORY_ROOT/save-points/step-02/start/. $REPOSITORY_ROOT/workshop/
    ```

    ```powershell
    # PowerShell
    Remove-Item -Path $REPOSITORY_ROOT/workshop -Recurse -Force && `
        New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop -Force && `
        Copy-Item -Path $REPOSITORY_ROOT/save-points/step-02/start/* -Destination $REPOSITORY_ROOT/workshop -Recurse -Force
    ```




---

축하합니다! 에이전트 백엔드에 AG-UI 프로토콜을 활용해서 프론트엔드를 연결했습니다. 이제 다음 단계로 이동하세요!

👈 [01: Microsoft Agent Framework 사용해서 에이전트 개발하기](./01-agent-with-maf.md) | [03: MCP 서버 개발하기](./03-mcp-server-development.md) 👉
