XDOCK_YAML=crossdock/docker-compose.yml

.PHONY: crossdock
crossdock:

	docker build -f crossdock/Dockerfile -t test .

	docker-compose -f $(XDOCK_YAML) kill csharp
	docker-compose -f $(XDOCK_YAML) rm -f csharp
	docker-compose -f $(XDOCK_YAML) build csharp
	docker-compose -f $(XDOCK_YAML) run crossdock

.PHONY: crossdock-fresh
crossdock-fresh:
	docker-compose -f $(XDOCK_YAML) down --rmi all
	docker-compose -f $(XDOCK_YAML) run crossdock

.PHONE: crossdock-logs
crossdock-logs:
	docker-compose -f $(XDOCK_YAML) logs
