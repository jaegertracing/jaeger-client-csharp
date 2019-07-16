XDOCK_YAML=crossdock/docker-compose.yml

.PHONY: dotnet-build
dotnet-build:
	dotnet build -c Release
	dotnet publish -c Release

.PHONY: crossdock
crossdock: dotnet-build
	docker-compose -f $(XDOCK_YAML) kill csharp
	docker-compose -f $(XDOCK_YAML) rm -f csharp
	docker-compose -f $(XDOCK_YAML) build csharp
	docker-compose -f $(XDOCK_YAML) run crossdock

.PHONY: crossdock-fresh
crossdock-fresh: dotnet-build
	docker-compose -f $(XDOCK_YAML) down --rmi all
	docker-compose -f $(XDOCK_YAML) run crossdock

.PHONE: crossdock-logs
crossdock-logs:
	docker-compose -f $(XDOCK_YAML) logs
