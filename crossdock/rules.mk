XDOCK_YAML=crossdock/docker-compose.yml

JAEGER_COMPOSE_URL=https://raw.githubusercontent.com/jaegertracing/jaeger/master/crossdock/jaeger-docker-compose.yml
XDOCK_JAEGER_YAML=crossdock/jaeger-docker-compose.yml

.PHONY: dotnet-build
dotnet-build:
	dotnet build -c Release
	dotnet publish -c Release

.PHONY: crossdock
crossdock: dotnet-build crossdock-download-jaeger
	docker-compose -f $(XDOCK_YAML) -f $(XDOCK_JAEGER_YAML) kill csharp
	docker-compose -f $(XDOCK_YAML) -f $(XDOCK_JAEGER_YAML) rm -f csharp
	docker-compose -f $(XDOCK_YAML) -f $(XDOCK_JAEGER_YAML) build csharp
	docker-compose -f $(XDOCK_YAML) -f $(XDOCK_JAEGER_YAML) run crossdock

.PHONY: crossdock-fresh
crossdock-fresh: dotnet-build crossdock-download-jaeger
	docker-compose -f $(XDOCK_JAEGER_YAML) -f $(XDOCK_YAML) down --rmi all
	docker-compose -f $(XDOCK_JAEGER_YAML) -f $(XDOCK_YAML) run crossdock

.PHONE: crossdock-logs
crossdock-logs: crossdock-download-jaeger
	docker-compose -f $(XDOCK_JAEGER_YAML) -f $(XDOCK_YAML) logs

.PHONY: crossdock-download-jaeger
crossdock-download-jaeger:
	curl -o $(XDOCK_JAEGER_YAML) $(JAEGER_COMPOSE_URL)
