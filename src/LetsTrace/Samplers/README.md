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
var maxTracesPerSecond = 10;
var sampler = new RateLimitingSampler(maxTracesPerSecond); // allow, at most, 10 traces through per second.
```

## Probabilistic Sampler
A sampler that randomly samples a certain percentage of traces.

```C#
var samplingRate = 0.25;
var sampler = new ProbabilisticSampler(samplingRate); // sample 25% of all traces
```

## Guaranteed Throughput Probabilistic Sampler
A sampler that leverages both [ProbabilisticSampler](#probabilistic-sampler) and [RateLimitingSampler](#rate-limiting-sampler). The `RateLimitingSampler` is used as a guaranteed lower bound sampler such that every operation is sampled at least once in a time interval defined by the lowerBound. ie a lowerBound of 1.0 / (60 * 10) will sample an operation at least once every 10 minutes.

```C#
var samplingRate = 0.25;
var lowerBound = 10;
var sampler = new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);
```

## Per-Operation Sampler
A sampler that uses the name of the operation to maintain a specific GuaranteedThroughputProbabilisticSampler instance for each operation up to a max number of operations. Any operation over the max number of operations uses a shared probabilistic sampler.

```C#
var maxOperations = 20; // number of specific operations to maintain a separate sampler instance for
var samplingRate = 0.25;
var lowerBound = 10;
var sampler = new PerOperationSampler(maxOperations, samplingRate, lowerBound);
```
