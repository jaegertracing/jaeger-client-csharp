FROM microsoft/windowsservercore@sha256:c06b4bfaf634215ea194e6005450740f3a230b27c510cf8facab1e9c678f3a99 as build

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ENV DOTNET_SDK_VERSION 2.2.203
RUN Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$Env:DOTNET_SDK_VERSION/dotnet-sdk-$Env:DOTNET_SDK_VERSION-win-x64.zip; \
Expand-Archive dotnet.zip -DestinationPath c:\sdk; \
Remove-Item -Force dotnet.zip

ENV ASPNETCORE_VERSION 2.2.6
RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$Env:ASPNETCORE_VERSION/aspnetcore-runtime-$Env:ASPNETCORE_VERSION-win-x64.zip; \
Expand-Archive aspnetcore.zip -DestinationPath c:\dotnet; \
Remove-Item -Force aspnetcore.zip

RUN $env:PATH = $('c:\sdk;{0}' -f $env:PATH) ;\
[Environment]::SetEnvironmentVariable('PATH', $env:PATH, [EnvironmentVariableTarget]::Machine)

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./crossdock/Jaeger.Crossdock/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./crossdock/Jaeger.Crossdock/. ./
RUN dotnet publish -c Release -o out



FROM microsoft/nanoserver@sha256:8f78a4a7da4464973a5cd239732626141aec97e69ba3e4023357628630bc1ee2 as runtime

EXPOSE 8080-8082

WORKDIR /app
COPY --from=build /dotnet /dotnet
COPY --from=build /app/out .

ENV \
DOTNET_RUNNING_IN_CONTAINER=true \
AGENT_HOST_PORT=jaeger-agent:5775 \
SAMPLING_SERVER_URL=http://test_driver:5778/sampling

WORKDIR /dotnet
ENTRYPOINT ["dotnet", "/app/Jaeger.Crossdock.dll"]