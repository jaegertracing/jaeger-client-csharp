#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["crossdock/Jaeger.Crossdock/Jaeger.Crossdock.csproj", "crossdock/Jaeger.Crossdock/"]
COPY ["src/Jaeger/Jaeger.csproj", "src/Jaeger/"]
COPY ["src/Senders/Jaeger.Senders.Thrift/Jaeger.Senders.Thrift.csproj", "src/Senders/Jaeger.Senders.Thrift/"]
COPY ["src/Encoders/Jaeger.Encoders.Thrift/Jaeger.Encoders.Thrift.csproj", "src/Encoders/Jaeger.Encoders.Thrift/"]
COPY ["src/Encoders/Jaeger.Encoders.SizedBatch/Jaeger.Encoders.SizedBatch.csproj", "src/Encoders/Jaeger.Encoders.SizedBatch/"]
COPY ["src/Jaeger.Core/Jaeger.Core.csproj", "src/Jaeger.Core/"]
COPY ["src/Transports/Jaeger.Transports.Thrift/Jaeger.Transports.Thrift.csproj", "src/Transports/Jaeger.Transports.Thrift/"]
COPY ["src/Communication/Jaeger.Communication.Thrift/Jaeger.Communication.Thrift.csproj", "src/Communication/Jaeger.Communication.Thrift/"]
COPY ["src/Senders/Jaeger.Senders.SizedBatch/Jaeger.Senders.SizedBatch.csproj", "src/Senders/Jaeger.Senders.SizedBatch/"]
RUN dotnet restore "crossdock/Jaeger.Crossdock/Jaeger.Crossdock.csproj"
COPY . .
WORKDIR "/src/crossdock/Jaeger.Crossdock"
RUN dotnet build "Jaeger.Crossdock.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jaeger.Crossdock.csproj" -c Release -o /app/publish

FROM base AS final
EXPOSE 8080-8082
WORKDIR /app
COPY --from=publish /app/publish .
ENV \
DOTNET_RUNNING_IN_CONTAINER=true \
AGENT_HOST_PORT=jaeger-agent:5775 \
SAMPLING_SERVER_URL=http://test_driver:5778/sampling
ENTRYPOINT ["dotnet", "Jaeger.Crossdock.dll"]