# Thrift senders for Jaeger (Jaeger.Senders.Thrift)

## Usage
This package contains the `ThriftSenderFactory` for use with `SenderResolver`.

This is the default sender, so in most cases you want to include the following line before doing any other calls to the `Configuration` API.

```C#
Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
	// You can add other ISenderFactory instances too
	.RegisterSenderFactory<ThriftSenderFactory>();
```

## Configuration
Depending on what variables are set in `Configuration.SenderConfiguration`, either a `HttpSender` or an `UdpSender` is generated. Setting the `SenderConfiguration.Endpoint` will allways result in an `HttpSender`.

```C#
new SenderConfiguration(loggerFactory)
	// Used with UdpSender:
	.WithAgentHost(agentHost)           // UDP hostname of agent
	.WithAgentPort(agentPort)           // UDP port of agent

	// Used with HttpSender:
	.WithEndpoint(collectorEndpoint)    // The traces endpoint of the collector
	.WithAuthToken(authToken)           // Authenticated token, to send as "Bearer" authentication
	.WithAuthUsername(authUsername)     // Username, to send as part of "Basic" authentication
	.WithAuthPassword(authPassword);    // Password, to send as part of "Basic" authentication
```

## Contained Senders
This package contains two senders. `UdpSender` for communicating with the `Jaeger Agent` and `HttpSender` for communicating with the `Jaeger Collector`.

In containerized environments, you usually want to talk to the `Jaeger Agent` via UDP for fast communication with little overhead. The `Jaeger Agent` then aggregates the data and relays them to the `Jaeger Collector`.

If you only have little instances or are running on the same machine, you might prefer to communicate directly with the `Jaeger Collector`.

### UdpSender
Communicates with the `Jaeger Agent`. This is the default variant of communication for Jaeger.

The default values are as follows:

Setting | Default
--- | ---
agentHost | localhost
agentPort | 6831

### HttpSender
Communicates with the `Jaeger Collector`. This is usually not used directly in most use-cases. The sender offers authentication through either `Basic` authentication, `Bearer` authentication or no authentication at all.

The default values are as follows:

Setting | Default
--- | ---
collectorEndpoint | _(none, must be set to use `HttpSender`)_
authToken | _(none)_
authUsername | _(none)_
authPassword | _(none)_
