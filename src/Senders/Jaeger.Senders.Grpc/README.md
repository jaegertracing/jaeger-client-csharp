# gRPC sender for Jaeger (Jaeger.Senders.GRPC)

## Usage
This package contains the `GrpcSenderFactory` for use with `SenderResolver`.

To use this, include the following line before doing any other calls to the `Configuration` API.

```C#
Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
	// You can add other ISenderFactory instances too
	.RegisterSenderFactory<GrpcSenderFactory>();
```

If you need to set `JAEGER_SENDER_FACTORY` (when multiple factories are registered), `GrpcSenderFactory.Name` corresponds to `"grpc"`.

## Configuration
Independent on what variables are set in `Configuration.SenderConfiguration`, a `GrpcSender` is created.

```C#
new SenderConfiguration(loggerFactory)
	// Used with GrpcSender:
	.WithGrpcTarget(grpcTarget)                    // Host and port or any other valid gRPC target
	.WithGrpcRootCertificate(grpcRootCertificate); // Path to the root certificate
```

## Contained Senders
This package contains one sender. `GrpcSender` for communicating with the `Jaeger Collector`.

In containerized environments, you usually want to talk to the `Jaeger Agent` via UDP for fast communication with little overhead. This package does not allow communication with `Jaeger Agent`. So if you require that, use the `ThriftSenderFactory` instead.

### GrpcSender
Communicates with the `Jaeger Collector`. This sender offers authentication through client certificate using `GrpcRootCertificate`.

The default values are as follows:

Setting | Default
--- | ---
grpcTarget | _(none)_
grpcRootCertificate | _(none)_