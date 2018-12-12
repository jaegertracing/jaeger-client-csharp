# Metrics: Prometheus
Use this add-on to use Prometheus as metrics endpoint.

## Adding the metrics factory
Just create an instance of `PrometheusMetricsFactory` to the `Tracer.Builder`.

```csharp
using Jaeger;
using Jaeger.Metrics;
using Microsoft.Extensions.Logging;

var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var serviceName = "initExampleService";

var metricsFactory = new PrometheusMetricsFactory();
var tracer = new Tracer.Builder(serviceName)
	.WithMetricsFactory(metricsFactory)
	.Build();
```

## Setup of the endpoint
Prometheus is an in-memory dimensional time series database. Only adding the metrics factory does not publish any data. To publish the data, either offer them through an HTTP endpoint or push it to an gateway. The following examples are taken from [prometheus-net](https://github.com/prometheus-net/prometheus-net). This is not done automatically and has to be done in your application.

### HTTP handler

Metrics are usually exposed over HTTP, to be read by the Prometheus server. The default metric server uses HttpListener to open up an HTTP API for metrics export.

```csharp
var metricServer = new MetricServer(port: 1234);
metricServer.Start();
```

The default configuration will publish metrics on the /metrics URL.

`MetricServer.Start()` may throw an access denied exception on Windows if your user does not have the right to open a web server on the specified port. You can use the *netsh* command to grant yourself the required permissions:

> netsh http add urlacl url=http://+:1234/metrics user=DOMAIN\user

### Pushgateway support

Metrics can be posted to a Pushgateway server over HTTP.

```csharp
var metricServer = new MetricPusher(endpoint: "http://pushgateway.example.org:9091/metrics", job: "some_job");
metricServer.Start();