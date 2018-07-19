[![Build status][ci-img]][ci] [![NuGet][nuget-img]][nuget]

# C# Client for Jaeger (https://jaegertracing.io)

- Implements C# [OpenTracing API](https://github.com/opentracing/opentracing-csharp)
- Supports netstandard 2.0

## Status
This library is still under construction and needs to be peer reviewed as well as have features added.

## Usage
This package contains everything you need to get up and running. If you want to report to a system such as Jaeger or Zipkin you will need to use their NuGet packages.

### The Tracer
The following will give you a tracer that reports spans to an `ILogger` instance from `ILoggerFactory`.

```C#
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;

var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var serviceName = "initExampleService";

var reporter = new LoggingReporter(loggerFactory);
var sampler = new ConstSampler(true);
var tracer = new Tracer.Builder(serviceName)
    .WithLoggerFactory(loggerFactory)
    .WithReporter(reporter)
    .WithSampler(sampler)
    .Build();
```

This works well if you only want to log to a logging framework. As soon as you want to also get metrics and use a real remote tracer, manually building will get hard pretty fast.

`Configuration` holds only primitive values and it is designed to be used with configuration files or when configuration is provided in environmental variables.

```C#
using Jaeger;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;

var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var serviceName = "initExampleService";

Configuration config = new Configuration("myServiceName")
	.WithSampler(...)   // optional, defaults to RemoteControlledSampler with HttpSamplingManager on localhost:5778
	.WithReporter(...); // optional, defaults to RemoteReporter with UdpSender on localhost:6831
```

ITracer tracer = config.GetTracer();

The config objects lazily builds and configures Jaeger Tracer. Multiple calls to GetTracer() return the same instance.

#### Configuration via Environment

It is also possible to obtain a `Jaeger.Configuration` object configured using properties specified
as environment variables or system properties. A value specified as a system property will override a value
specified as an environment variable for the same property name.

```C#
Configuration config = Configuration.FromEnv();
```

The property names are:

Property | Required | Description
--- | --- | ---
JAEGER_SERVICE_NAME | yes | The service name
JAEGER_AGENT_HOST | no | The hostname for communicating with agent via UDP
JAEGER_AGENT_PORT | no | The port for communicating with agent via UDP
JAEGER_ENDPOINT | no | The traces endpoint, in case the client should connect directly to the Collector, like http://jaeger-collector:14268/api/traces
JAEGER_AUTH_TOKEN | no | Authentication Token to send as "Bearer" to the endpoint
JAEGER_USER | no | Username to send as part of "Basic" authentication to the endpoint
JAEGER_PASSWORD | no | Password to send as part of "Basic" authentication to the endpoint
JAEGER_PROPAGATION | no | Comma separated list of formats to use for propagating the trace context. Defaults to the standard Jaeger format. Valid values are **jaeger** and **b3**
JAEGER_REPORTER_LOG_SPANS | no | Whether the reporter should also log the spans
JAEGER_REPORTER_MAX_QUEUE_SIZE | no | The reporter's maximum queue size
JAEGER_REPORTER_FLUSH_INTERVAL | no | The reporter's flush interval (ms)
JAEGER_SAMPLER_TYPE | no | The sampler type
JAEGER_SAMPLER_PARAM | no | The sampler parameter (number)
JAEGER_SAMPLER_MANAGER_HOST_PORT | no | The host name and port when using the remote controlled sampler
JAEGER_TAGS | no | A comma separated list of `name = value` tracer level tags, which get added to all reported spans. The value can also refer to an environment variable using the format `${envVarName:default}`, where the `:default` is optional, and identifies a value to be used if the environment variable cannot be found
JAEGER_TRACEID_128BIT | no | Whether to use 128bit TraceID instead of 64bit

Setting `JAEGER_AGENT_HOST`/`JAEGER_AGENT_PORT` will make the client send traces to the agent via `UdpSender`.
If the `JAEGER_ENDPOINT` environment variable is also set, the traces are sent to the endpoint, effectively making
the `JAEGER_AGENT_*` vars ineffective.

When the `JAEGER_ENDPOINT` is set, the `HttpSender` is used when submitting traces to a remote
endpoint, usually served by a Jaeger Collector. If the endpoint is secured, a HTTP Basic Authentication
can be performed by setting the related environment vars. Similarly, if the endpoint expects an authentication
token, like a JWT, set the `JAEGER_AUTH_TOKEN` environment variable. If the Basic Authentication environment
variables *and* the Auth Token environment variable are set, Basic Authentication is used.

#### Reporting
For more information on reporting see the reporting [README](src/Jaeger/Reporters/README.md)

#### Sampling
For more information on sampling see the sampling [README](src/Jaeger/Samplers/README.md)

#### Extracting Span Information
When your code is called you might want to pull current trace information out of calling information before building and starting a span. This allows you to link your span into a current trace and track its relation to other spans. By default text map and http headers are supported. More support is planned for the future as well as allowing custom extractors.

```C#
using OpenTracing.Propagation; // where you get Format from

var callingHeaders = new TextMapExtractAdapter(...); // get the calling headers

var callingSpanContext = tracer.Extract(BuiltinFormats.HttpHeaders, callingHeaders);
```
You can then use the callingSpanContext when [adding references](#adding-references) with the SpanBuilder.

#### Injecting Span Information
In order to pass along the trace information in calls so others can extract it you need to inject it into the carrier.

```C#
using OpenTracing.Propagation; // where you get BuiltinFormats from

var spanContext = span.Context; // pulled from your current span
var newCallHeaders = new TextMapInjectAdapter(null); // get the calling headers

tracer.Inject(spanContext, BuiltinFormats.HttpHeaders, newCallHeaders);
```
You can then pass along the headers and as along as what you are calling knows how to extract that format you are good to go.

### Building a Span
Before you start a span you will want to build it out. You can do this using the span builder. You would build a span for each operation you wanted to trace.

```C#
var operationName = "Get::api/values/";
var builder = tracer.BuildSpan(operationName);
```

#### Adding Tags
Any tags you add to the span builder will be added to the span on start and reported to the reporting system you have setup when the span is reported. The following types are supported as tags: `bool`, `double`, `int`, `string`.

```C#
builder.WithTag("machine.name", "machine1").WithTag("cpu.cores", 8);
```

Some well-known tags are defined in `OpenTracing.Tag` and can be used as follows:

```C#
using OpenTracing.Tag;

builder.WithTag(Tags.SpanKind, Tags.SpanKindClient).WithTag(Tags.DbType, "sql");
```

#### Adding References
References allow you to show how this span relates to another span. You need the `SpanContext` of the span you want to reference. If you add a `child_of` reference the SpanBuilder will use that as the parent of the span being built.

```C#
builder.AddReference(References.FollowsFrom, spanContext);
```
There also exist helper methods to simplify adding child of references.

#### As Child Of
Shorthand for adding a chold of reference. You can pass in an `ISpan` or and `ISpanContext`.

```C#
builder.AsChildOf(iSpanOrISpanContext);
```

#### Starting the Span
Starting the span from the span builder will figure out if there is a parent for the span, create a context for the span, and pass along all references and tags.

You can start the span right now:
```C#
var span = builder.Start();
```

Or you can start it at a specific time:
```C#
var startTime = DateTimeOffset.Now;
var span = builder.WithStartTimestamp(startTime).Start();
```

If you want to start a span and use it as an active span, you can use a scoped span.
```C#
using (var scope = builder.StartActive(true))
{
	var span = scope.Span;
}
```

This will automatically define the newly created span as child of the span that was active at that time. If no span was active, it will be created as root span. 
In addition will the scope span be automatically finished when the scope ends, even if the `using`-Block throws an exception.

### Spans
After creating a span and before finishing it, you can add and change some information on a span.

#### Baggage Items
Baggage is key/value data that is passed along the wire and shared with other spans. You can get and set baggage data from the span object.

```C#
var mobileVersion = span.GetBaggageItem("mobile.version");
```

```C#
span.SetBaggageItem("back-end.version", "0.0.1");
```

#### Logging
You can log structured data which allows you to tie information from what's happening along the lifetime of a span to the time that it happened. You can log a list of key/value data or an event at a specific time.

```C#
var logData = new List<KeyValuePair<string, object>> {
    { "handling number of events", 6 },
    { "using legacy system", false }
};

span.Log(DateTimeOffset.Now, logData);
```
or you can pass it without a timestamp and the timestamp will be sent for you:
```C#
span.Log(logData);
```

Events are a little different in that they're just a string.
```C#
var event = "loop_finished";

span.Log(DateTimeOffset.Now, event);
```
and as above you can send an event in without a timestamp:
```C#
span.Log(event);
```

#### Tags
Tags can be set using `SetTag(<key>, <value>)` and follows the builder [WithTag](#adding-tags) in the data types it accepts.

#### Operation Name
You can change the operation name from what was originally set on the span when it was created.

```C#
span.SetOperationName("PUT::api/values/");
```

#### Finishing
`Span` implements `IDisposable` so a using statement will automatically finish your span. However, you can also call `Finish`. You can either pass in the finish time or let the library handle that for you.

```C#
span.Finish(DateTimeOffset.Now);
```
or
```C#
span.Finish();
```

## Contributing

We welcome community contributions to this project. Please see [CONTRIBUTING.md](./CONTRIBUTING.md) for more details.

By contributing your code, you agree to license your contribution under the terms of the [APLv2](LICENSE).

## License

All files are released with the [Apache 2.0 license](LICENSE).

[ci-img]: https://ci.appveyor.com/api/projects/status/github/jaegertracing/jaeger-client-csharp?svg=true
[ci]: https://ci.appveyor.com/project/jaegertracing/jaeger-client-csharp
[nuget-img]: https://img.shields.io/nuget/v/Jaeger.svg
[nuget]: https://www.nuget.org/packages/Jaeger/
