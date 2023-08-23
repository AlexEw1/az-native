# Container Recap (Multistage Build, Run, Debug, Publish to ACR)

## Build & Publish Config Api & Config UI - Simple 2-tier app

Remove existing `node_modules` and `.angular` folders in `config-ui`, if present to reduce upload time to Azrue container registry.

Execute `acr-build.azcli` to build and publish to Azure Container Registry.

## Docker Development Workflow and Debugging

Examine debug.dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["config-api.csproj", "./"]
RUN dotnet restore "config-api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "config-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "config-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "config-api.dll"]
```

Ensure that a debug configuration is present. If not create it using the F1 command: 

![generate-assets](_images/generate-assets.png)

Use the [Docker - Visual Studio Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker) to create a Docker Debug Configuration:

![docker-ext](_images/docker-ext.png)

Update the `docker-build` task in `tasks.json` to use `debug.dockerfile`:

```json
{
        "type": "docker-run",
        "label": "docker-run: debug",
        "dependsOn": [
            "docker-build: debug"
        ],
        "dockerRun": {
            "ports": [{"hostPort": 5050, "containerPort": 80}],
            "env": {
                "App__MockSetting":"ChangedMockValue",
            }
        },
        "netCore": {
            "appProject": "${workspaceFolder}/config-api.csproj",
            "enableDebugging": true
        }
    },
```

For container debugging customize `docker-run: debug` in `.vscode/tasks.json`. Add a mock environment variable:

```json
{
    "type": "docker-run",
    "label": "docker-run: debug",
    "dependsOn": [
        "docker-build: debug"
    ],
    "dockerRun": {
        "ports": [{"hostPort": 5050, "containerPort": 80}],
        "env": {
            "App__MockSetting":"ChangedMockValue",
        }
    },
    "netCore": {
        "appProject": "${workspaceFolder}/config-api.csproj",
        "enableDebugging": true
    }
},
```

Run the DockerDebug configuration and notice that the overrided value for the `MockSetting` is returned.

`Attach to shell` and use `printenv` to show the variables in the container:

![attach](_images/attach.png)