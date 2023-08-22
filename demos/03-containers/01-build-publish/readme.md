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

Use the [Docker - Visual Studio Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker) to create a Docker Debug Configuration:

![docker-ext](_images/docker-ext.png)

Update the `docker-build` task `dockerfile` prop to use `debug.dockerfile`:

```json
{
    "type": "docker-build",
    "label": "docker-build: debug",
    "dependsOn": [
        "build"
    ],
    "dockerBuild": {
        "tag": "config-api:dev",
        "target": "base",
        "dockerfile": "${workspaceFolder}/debug.dockerfile",
        "context": "${workspaceFolder}",
        "pull": true,
    },
    "netCore": {
        "appProject": "${workspaceFolder}/config-api.csproj"
    },
},
```

For container debugging customize `docker-run: debug` in `.vscode/tasks.json`:

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
            "App__UseEnv":"true",
            "Azure__TenantId":"d92b0000-90e0-4469-a129-6a32866c0d0a"
        }
    },
    "netCore": {
        "appProject": "${workspaceFolder}/net-env-vars.csproj",
        "enableDebugging": true
    }
},
```

Set container environment variables:

```json
"env": {            
    "App__UseEnv":"true",
    "Azure__TenantId":"d92b0000-90e0-4469-a129-6a32866c0d0a"
}
```

>Note: `Azure__TenantId` mimics the structure of `appsettings.json`:

```json
{ 
  "Azure": {
  "TenantId": "d92b247e-90e0-4469-a129-6a32866c0d0a",
```

Set the port mapping:

```json
"ports": [{"hostPort": 5050, "containerPort": 80}],
```

Set your startup url in `launch.json` to route to the `SettingsController` using `%s://localhost:%s/settings`:

```json
{
    "name": "Docker Debug",
    "type": "docker",
    "request": "launch",
    "preLaunchTask": "docker-run: debug",
    "netCore": {
        "appProject": "${workspaceFolder}/net-env-vars.csproj"
    },
    "dockerServerReadyAction": {
        "uriFormat": "%s://localhost:%s/settings"
    }
}
```

Notice that the overrided value for the `TenantId` is returned:

![tenantid](_images/tenantid.png)

`Attach to shell` and use `printenv` to show the variables in the container:

![attach](_images/attach.png)

Build and publish image:

```bash
docker build --rm -f dockerfile -t net-env-vars:debug .
docker tag net-env-vars:debug arambazamba/net-env-vars:debug
docker push arambazamba/net-env-vars:debug
```

Test in [Azure Container Instances](https://docs.microsoft.com/en-us/azure/container-instances/) by executing `../deploy-to-aci.azcli`:

```bash
rnd=$RANDOM
grp=az204-m05-ci-env-$rnd
loc=westeurope
app=net-env-vars-$RANDOM
img=arambazamba/net-env-vars:debug

az group create -n $grp -l $loc

az container create -g $grp -l $loc -n $app --image $img --cpu 1 --memory 1 --dns-name-label $app --port 80 --environment-variables 'App__UseEnv'='true' 'Azure__TenantId'='d9101010-90e0-4469-a129-6a32866c0d0a' --query ipAddress.fqdn -o tsv
```