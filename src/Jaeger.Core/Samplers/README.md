# Sampling
Use sampling to keep from reporting every single span. When you're working with a large volume of calls, reporting tracing information on every single one can be resource intensive and you might want to only report some of them. Using the different samplers here you can report different ways.

## Const Sampler
A sampler that always makes the same decision.

```C#
var sampler = new ConstSampler(true); // always send the span
```

## Rate Limiting Sampler 
A sampler that samples traces and allows a set number of traces through per second.

```C#
var maxTracesPerSecond = 10d;
var sampler = new RateLimitingSampler(maxTracesPerSecond); // allow, at most, 10 traces through per second.
```

## Probabilistic Sampler
A sampler that randomly samples a certain percentage of traces.

```C#
var samplingRate = 0.25d;
var sampler = new ProbabilisticSampler(samplingRate); // sample 25% of all traces
```

## Guaranteed Throughput Probabilistic Sampler
A sampler that leverages both [ProbabilisticSampler](#probabilistic-sampler) and [RateLimitingSampler](#rate-limiting-sampler). The `RateLimitingSampler` is used as a guaranteed lower bound sampler such that every operation is sampled at least once in a time interval defined by the lowerBound. ie a lowerBound of 1.0 / (60 * 10) will sample an operation at least once every 10 minutes.

```C#
var samplingRate = 0.25d;
var lowerBound = 10d;
var sampler = new GuaranteedThroughputSampler(samplingRate, lowerBound);
```

## Per-Operation Sampler
A sampler that uses the name of the operation to maintain a specific `GuaranteedThroughputSampler` instance for each operation up to a max number of operations. Any operation over the max number of operations uses a shared probabilistic sampler.

```C#
var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var maxOperations = 20; // number of specific operations to maintain a separate sampler instance for
var defaultSamplingProbability = 0.25d;
var defaultLowerBound = 10d;
var samplingRate = 0.25d;
var perOperationStrategies = new List<PerOperationSamplingParameters>
{
    new PerOperationSamplingParameters("operation1", new ProbabilisticSamplingStrategy(samplingRate)),
    ...
};
var strategies = new OperationSamplingParameters(defaultSamplingProbability, defaultLowerBound, perOperationStrategies);
var sampler = new PerOperationSampler(maxOperations, strategies, loggerFactory);
```

## Remote Controlled Sampler
A sampler that retrieves it's configuration from an `ISamplingManager` instance. The configuration is updated by a given `PollingInterval` on the sampler. Supported are `ProbabilisticSampler`, `RateLimitingSampler` and `PerOperationSampler`. Until the first response was fetched and as fallback on network problems, a `ProbabilisticSampler` is used.

```C#
var loggerFactory = ; // get Microsoft.Extensions.Logging ILoggerFactory
var sampler = new RemoteControlledSampler.Builder("myServiceName")
    .WithLoggerFactory(loggerFactory) // optional, defaults to no logging
    .WithInitialSampler(...)          // optional, defaults to ProbabilisticSampler(0.001D)
    .WithPollingInterval(...)         // optional, defaults to TimeSpan.FromMinutes(1)
    .WithSamplingManager(...)         // optional, defaults to HttpSamplingManager("localhost:5778")
    .Build();
```
