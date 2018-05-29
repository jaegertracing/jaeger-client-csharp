# Reporting
Use reporters to define what happens when a span gets reported. Those only get triggered after the configured `Sampler` decided that it should be sampled.

## No-op Reporter
A reporter that does nothing. This is the default implementation used in `GlobalTracer.Instance`.

```C#
var reporter = new NoopReporter();
```

## In-memory Reporter
A reporter that stores the reported spans in-memory. The can later be retrieved as `List` or `string`.

```C#
var reporter = new InMemoryReporter();
...
var spans = reporter.GetSpans();
```

## Logging Reporter
A reporter that prints every reported span to the logging framework. It receives an `ILoggerFactory` instance and creates an `ILogger` instance that is used subsequently.

Spans will be reported with `LogLevel.Information`.

```C#
var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var reporter = new LoggingReporter(loggerFactory);
```

## Remote Reporter
A reporter that sends spans to a remote endpoint.

```C#
var reporter = new RemoteReporter.Builder()
    .WithLoggerFactory(loggerFactory) // optional, defaults to no logging
    .WithMaxQueueSize(...)            // optional, defaults to 100
    .WithFlushInterval(...)           // optional, defaults to TimeSpan.FromSeconds(1)
    .WithSender(...)                  // optional, defaults to UdpSender("localhost", 6831, 0)
    .Build();
```

## Combined Reporter
A reporter that combines multiple reporters for usage with the tracer. This is mostly used for debugging, when an output to the Console logger is wanted.

```C#
var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var reporterLogging = new LoggingReporter(loggerFactory);
var reporterInMemory = new InMemoryReporter();
var reporterRemote = new RemoteReporter.Builder()
    ...
    .Build();
var reporter = new CompositeReporter(reporterLogging, reporterInMemory, reporterRemote);
...
var spans = reporterInMemory.GetSpans();
```

# Notice
Most of the time, configuration will happen through usage of the `Configuration` helper. A common example for getting a combined `LoggingReporter` and `RemoteReporter` can be achieved by using

```C#
var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
    .With...();

var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
    .WithSender(senderConfiguration) // optional, defaults to UdpSender at localhost:6831
    .WithLogSpans(true);             // optional, defaults to no LoggingReporter

var tracer = new Configuration(serviceName, loggerFactory)
    .WithSampler(...)                    // optional, defaults to RemoteControlledSampler with HttpSamplingManager on localhost:5778
    .WithReporter(reporterConfiguration) // optional, defaults to RemoteReporter with UdpSender at localhost:6831
    .GetTracer();
```

See general [README](../../../README.md) for more information on `Configuration` on getting configuration from the environment.
