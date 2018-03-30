using System.Collections.Generic;
using Jaeger.Transport.Thrift.Samplers.HTTP;
using Xunit;
using JaegerThriftOperationSamplingStrategy = Jaeger.Thrift.Agent.OperationSamplingStrategy;
using JaegerThriftProbabilisticSamplingStrategy = Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy;
using JaegerThriftPerOperationSamplingStrategies = Jaeger.Thrift.Agent.PerOperationSamplingStrategies;
using JaegerThriftRateLimitingSamplingStrategy = Jaeger.Thrift.Agent.RateLimitingSamplingStrategy;
using JaegerThriftSamplingStrategyResponse = Jaeger.Thrift.Agent.SamplingStrategyResponse;

namespace Jaeger.Transport.Thrift.Tests.Samplers.Http
{
    public class MarshallersTests
    {
        [Fact]
        public void JaegerThriftOperationSamplingStrategy_FromThrift()
        {
            var pss = new JaegerThriftProbabilisticSamplingStrategy(0.75);
            var strat = new JaegerThriftOperationSamplingStrategy("opName", pss);

            var from = strat.FromThrift();

            Assert.Equal(strat.Operation, from.Operation);
            Assert.Equal(pss.SamplingRate, from.ProbabilisticSampling.SamplingRate);
        }

        [Fact]
        public void JaegerThriftPerOperationSamplingStrategies_FromThrift()
        {
            var opStrats = new List<JaegerThriftOperationSamplingStrategy>
            {
                new JaegerThriftOperationSamplingStrategy("op1", new JaegerThriftProbabilisticSamplingStrategy(0.5)),
                new JaegerThriftOperationSamplingStrategy("op2", new JaegerThriftProbabilisticSamplingStrategy(0.45))
            };
            var strat = new JaegerThriftPerOperationSamplingStrategies(0.25, 10, opStrats);

            var from = strat.FromThrift();

            Assert.Equal(strat.DefaultSamplingProbability, from.DefaultSamplingProbability);
            Assert.Equal(strat.DefaultLowerBoundTracesPerSecond, from.DefaultLowerBoundTracesPerSecond);
            Assert.Equal(opStrats[0].Operation, from.PerOperationStrategies[0].Operation);
            Assert.Equal(opStrats[0].ProbabilisticSampling.SamplingRate, from.PerOperationStrategies[0].ProbabilisticSampling.SamplingRate);
            Assert.Equal(opStrats[1].Operation, from.PerOperationStrategies[1].Operation);
            Assert.Equal(opStrats[1].ProbabilisticSampling.SamplingRate, from.PerOperationStrategies[1].ProbabilisticSampling.SamplingRate);
        }

        [Fact]
        public void JaegerThriftProbabilisticSamplingStrategy_FromThrift()
        {
            var strat = new JaegerThriftProbabilisticSamplingStrategy(0.25);

            var from = strat.FromThrift();

            Assert.Equal(strat.SamplingRate, from.SamplingRate);
        }

        [Fact]
        public void JaegerThriftRateLimitingSamplingStrategy_FromThrift()
        {
            var strat = new JaegerThriftRateLimitingSamplingStrategy(5);

            var from = strat.FromThrift();

            Assert.Equal(strat.MaxTracesPerSecond, from.MaxTracesPerSecond);
        }

        [Fact]
        public void JaegerThriftSamplingStrategyResponse_FromThrift()
        {
            var opStrats = new List<JaegerThriftOperationSamplingStrategy>
            {
                new JaegerThriftOperationSamplingStrategy("operation1", new JaegerThriftProbabilisticSamplingStrategy(0.5)),
                new JaegerThriftOperationSamplingStrategy("operation2", new JaegerThriftProbabilisticSamplingStrategy(0.45))
            };
            var perOpStrats = new JaegerThriftPerOperationSamplingStrategies(0.33, 18, opStrats);

            var strat = new JaegerThriftSamplingStrategyResponse
            {
                ProbabilisticSampling = new JaegerThriftProbabilisticSamplingStrategy(0.85),
                RateLimitingSampling = new JaegerThriftRateLimitingSamplingStrategy(25),
                OperationSampling = perOpStrats
            };

            var from = strat.FromThrift();

            Assert.Equal(strat.ProbabilisticSampling.SamplingRate, from.ProbabilisticSampling.SamplingRate);
            Assert.Equal(strat.RateLimitingSampling.MaxTracesPerSecond, from.RateLimitingSampling.MaxTracesPerSecond);

            Assert.Equal(perOpStrats.DefaultSamplingProbability, from.OperationSampling.DefaultSamplingProbability);
            Assert.Equal(perOpStrats.DefaultLowerBoundTracesPerSecond, from.OperationSampling.DefaultLowerBoundTracesPerSecond);
            Assert.Equal(opStrats[0].Operation, from.OperationSampling.PerOperationStrategies[0].Operation);
            Assert.Equal(opStrats[0].ProbabilisticSampling.SamplingRate, from.OperationSampling.PerOperationStrategies[0].ProbabilisticSampling.SamplingRate);
            Assert.Equal(opStrats[1].Operation, from.OperationSampling.PerOperationStrategies[1].Operation);
            Assert.Equal(opStrats[1].ProbabilisticSampling.SamplingRate, from.OperationSampling.PerOperationStrategies[1].ProbabilisticSampling.SamplingRate);
        }
    }
}
