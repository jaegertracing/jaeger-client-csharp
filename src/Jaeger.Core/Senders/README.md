# Senders
Use senders to define what happens when a span gets reported to a `RemoteReporter`.

## ISender
Defines the methods called by the `RemoteReporter`. Each instance of the `ISender` therefore handles the transport of the spans reported to the target system.

It is usually instantiated through `ISenderFactory.GetSender()` which receives an instance of `Configuration.SenderConfiguration` for configuring the sender.

## ISenderFactory
It has a fixed and unique `FactoryName` with that it's registered to the `SenderResolver`. If multiple `ISenderFactory` instances are registered to one `SenderResolver`, the configuration `JAEGER_SENDER_FACTORY` needs to be set to the `FactoryName` to select the targeted instance.

## SenderResolver
It resolves an `ISender` instance based on `Configuration.SenderConfiguration`. `Configuration.SenderConfiguration.SenderFactory` (usually coming from `JAEGER_SENDER_FACTORY`) is used to select an `ISenderFactory` by it's matching `FactoryName`. This is only needed if multiple `ISenderFactory` instances are registered to this `SenderResolver`.

If the factory to use is not found or was ambiguous due to no `SenderFactory` being defined, `NoopSender.Instance` is returned instead.

If a factory was found, `ISenderFactory.GetSender()` will be used to get a configured instance of `ISender` to be returned to the caller of `SenderResolver.Resolve()`.

```C#
var senderResolver = new SenderResolver(loggerFactory)
	.RegisterSenderFactory<ThriftSenderFactory>();
var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
	.WithSenderResolver(senderResolver)	// optional, defaults to Configuration.SenderConfiguration.DefaultSenderResolver
	.WithSenderFactory("thrift");		// optional if only one Factory registered to senderResolver
var sender = senderResolver.Resolve(senderConfiguration);
```

## Configuration.SenderConfiguration.DefaultSenderResolver
Usually, there only needs to be one `SenderResolver` for all `ITracer` per application. To ease configuration, set `Configuration.SenderConfiguration.DefaultSenderResolver` with your `SenderResolver` and register the wanted `ISenderFactory` instances to it.

```C#
Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
	.RegisterSenderFactory<ThriftSenderFactory>();

// This creates an HTTP sender through ThriftSenderFactory
var httpSender = new Configuration.SenderConfiguration(loggerFactory)
	.WithEndpoint("https://jaeger-collector:14268/api/traces")
	.WithAuthUsername("username")
	.WithAuthPassword("password")
	.GetSender();

// This creates an UDP sender through ThriftSenderFactory
var udpSender = new Configuration.SenderConfiguration(loggerFactory)
	.WithAgentHost("jaeger-agent")
	.WithAgentPort(6832)
	.GetSender();

// This creates a tracer, using an ISender generated through ThriftSenderFactory, depending on configuration
var tracer = Configuration.FromEnv(loggerFactory).GetTracer();
```

# Notice
The `ThriftSenderFactory` is defined as part of the NuGET package `Jaeger.Senders.Thrift`. This is usually included through the meta-package `Jaeger`. If you do not want to add a dependency on `Jaeger.Thrift.VendoredThrift` when using other `Jaeger.Senders.*` packages or when defining your own `ISender`/`ISenderFactory`, use the package `Jaeger.Core` directly instead of `Jaeger`.

By default, `Configuration.SenderConfiguration.DefaultSenderResolver` does NOT contain any `ISenderFactory` instances since `Jaeger.Core` is agnostic of any `ISender` implementation. All calls to `SenderResolver.Resolve` will return `NoopSender.Instance`.

Therefore, in most cases you want to include the following line before doing any other calls to the `Configuration` API.

```C#
Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
	.RegisterSenderFactory<ThriftSenderFactory>();
```