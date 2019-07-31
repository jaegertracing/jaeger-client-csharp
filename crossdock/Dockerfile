FROM microsoft/windowsservercore:1607 as build

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ENV ASPNETCORE_VERSION 2.2.4
RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$Env:ASPNETCORE_VERSION/aspnetcore-runtime-$Env:ASPNETCORE_VERSION-win-x64.zip; \
Expand-Archive aspnetcore.zip -DestinationPath c:\dotnet; \
Remove-Item -Force aspnetcore.zip



FROM mcr.microsoft.com/windows/nanoserver:sac2016 as runtime

EXPOSE 8080-8082

COPY --from=build /dotnet /dotnet

WORKDIR /app
COPY ./Jaeger.Crossdock/bin/Release/netcoreapp2.2/publish/* ./

ENV \
DOTNET_RUNNING_IN_CONTAINER=true \
AGENT_HOST_PORT=jaeger-agent:5775 \
SAMPLING_SERVER_URL=http://test_driver:5778/sampling

RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$Env:ASPNETCORE_VERSION/aspnetcore-runtime-$Env:ASPNETCORE_VERSION-win-x64.zip; \
Expand-Archive aspnetcore.zip -DestinationPath c:\dotnet; \
Remove-Item -Force aspnetcore.zip

WORKDIR /dotnet
ENTRYPOINT ["dotnet", "/app/Jaeger.Crossdock.dll"]