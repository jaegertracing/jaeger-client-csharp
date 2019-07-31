FROM microsoft/nanoserver@sha256:8f78a4a7da4464973a5cd239732626141aec97e69ba3e4023357628630bc1ee2 as runtime

EXPOSE 8080-8082

WORKDIR /app

COPY publish/. .

ENV \
DOTNET_RUNNING_IN_CONTAINER=true \
AGENT_HOST_PORT=jaeger-agent:5775 \
SAMPLING_SERVER_URL=http://test_driver:5778/sampling

WORKDIR /dotnet
ENTRYPOINT ["dotnet", "/app/Jaeger.Crossdock.dll"]