FROM microsoft/windowsservercore@sha256:c06b4bfaf634215ea194e6005450740f3a230b27c510cf8facab1e9c678f3a99 as build

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ENV ASPNETCORE_VERSION 2.2.6
RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$Env:ASPNETCORE_VERSION/aspnetcore-runtime-$Env:ASPNETCORE_VERSION-win-x64.zip; \
Expand-Archive aspnetcore.zip -DestinationPath c:\dotnet; \
Remove-Item -Force aspnetcore.zip



FROM microsoft/nanoserver@sha256:8f78a4a7da4464973a5cd239732626141aec97e69ba3e4023357628630bc1ee2 as runtime

EXPOSE 8080-8082

COPY --from=build /dotnet /dotnet
COPY ./publish /app

ENV \
DOTNET_RUNNING_IN_CONTAINER=true \
AGENT_HOST_PORT=jaeger-agent:5775 \
SAMPLING_SERVER_URL=http://test_driver:5778/sampling

WORKDIR /dotnet
ENTRYPOINT ["dotnet", "/app/Jaeger.Crossdock.dll"]