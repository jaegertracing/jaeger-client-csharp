[![Build status][ci-img]][ci] [![NuGet][nuget-img]][nuget]

# C# Client for Jaeger (https://jaegertracing.io)

- Implements C# [OpenTracing API](https://github.com/opentracing/opentracing-csharp) v. 0.12
- Supports netstandard 2.0

## Status
This library is still under construction and needs to be peer reviewed as well as have features added.

## Usage
This package contains everything you need to get up and running. If you want to report to a system such as Jaeger or Zipkin you will need to use their NuGet packages.

### The Tracer
The following will give you a tracer that reports spans to the `ILogger` from `ILoggerFactory`.

```C#
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;

var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory

var serviceName = "initExampleService";
var reporter = new LoggingReporter(logger);
var sampler = new ConstSampler(true);
var tracer = new Tracer.Builder(serviceName)
    .WithLoggerFactory(loggerFactory)
    .WithReporter(reporter)
    .WithSampler(sampler)
    .Build();
```

#### Sampling
For more information on sampling see the sampling [README](src/Jaeger/Samplers/README.md)

#### Extracting Span Information
When your code is called you might want to pull current trace information out of calling information before building and starting a span. This allows you to link your span into a current trace and track its relation to other spans. By default text map and http headers are supported. More support is planned for the future as well as allowing custom extractors.

```C#
using OpenTracing.Propagation; // where you get Format from

DictionaryTextMap callingHeaders = ; // get the calling headers

var callingSpanContext = tracer.Extract(Format.HttpHeaders, callingHeaders)
```
You can then use the callingSpanContext when [adding references](#adding-references) with the SpanBuilder.

#### Injecting Span Information
In order to pass along the trace information in calls so others can extract it you need to inject it into the carrier.

```C#
using OpenTracing.Propagation; // where you get Format from

var spanContext = span.Context; // pulled from your current span
DictionaryTextMap newCallHeaders; // get the calling headers

var callingSpanContext = tracer.Inject(spanContext, Format.HttpHeaders, newCallHeaders)
```
You can then pass along the headers and as along as what you are calling knows how to extract that format you are good to go.

### Building a Span
Before you start a span you will want to build it out. You can do this using the span builder. You would build a span for each operation you wanted to trace.

```C#
var operationName = "Get::api/values/";
var builder = tracer.BuildSpan(operationName);
```

#### Adding Tags
Any tags you add to the span builder will be added to the span on start and reported to the reporting system you have setup when the span is reported. The following types are supported as tags: bool, double, int, string.

```C#
builder.WithTag("machine.name", "machine1").WithTag("cpu.cores", 8);
```

#### Adding References
References allow you to show how this span relates to another span. You need the `SpanContext` of the span you want to reference. If you add a `child_of` reference the SpanBuilder will use that as the parent of the span being built.

```C#
builder.AddReference("follows_from", spanContext);
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

### Spans
After creating a span and before finishing it you can add and change some information on a span.

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
    new KeyValuePair<string, object>("handling number of events", 6),
    new KeyValuePair<string, object>("using legacy system", false)
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

[ci-img]: https://ci.appveyor.com/api/projects/status/evahkowja82u3sr4?svg=true
[ci]: https://ci.appveyor.com/project/jaegertracing/jaeger-client-csharp
[nuget-img]: https://img.shields.io/nuget/v/Jaeger.svg
[nuget]: https://www.nuget.org/packages/Jaeger/
