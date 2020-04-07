#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["crossdock/Jaeger.Crossdock/Jaeger.Crossdock.csproj", "crossdock/Jaeger.Crossdock/"]
COPY ["src/Jaeger/Jaeger.csproj", "src/Jaeger/"]
COPY ["src/Jaeger.Thrift/Jaeger.Thrift.csproj", "src/Jaeger.Thrift/"]
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