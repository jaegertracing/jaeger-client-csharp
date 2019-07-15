FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY ./Jaeger.Crossdock/bin/Release/netcoreapp2.2/publish/* ./

ENV AGENT_HOST_PORT=jaeger-agent:5775
ENV SAMPLING_SERVER_URL=http://test_driver:5778/sampling

EXPOSE 8080-8082

ENTRYPOINT ["dotnet", "Jaeger.Crossdock.dll"]