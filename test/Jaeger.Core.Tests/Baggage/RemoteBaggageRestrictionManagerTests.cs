using System;
using System.Collections.Generic;
using Jaeger.Core.Baggage;
using Jaeger.Core.Baggage.Http;
using Jaeger.Core.Metrics;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Baggage
{
    public class RemoteBaggageRestrictionManagerTest : IDisposable
    {
        private const string SERVICE_NAME = "service";
        private const string BAGGAGE_KEY = "key";
        private const int MAX_VALUE_LENGTH = 10;
        private static readonly BaggageRestrictionResponse RESTRICTION = new BaggageRestrictionResponse(BAGGAGE_KEY, MAX_VALUE_LENGTH);

        private readonly IBaggageRestrictionManagerProxy _baggageRestrictionProxy;
        private readonly IMetrics _metrics;
        private readonly InMemoryMetricsFactory _inMemoryMetricsFactory;

        private RemoteBaggageRestrictionManager _undertest;

        public RemoteBaggageRestrictionManagerTest()
        {
            _baggageRestrictionProxy = Substitute.For<IBaggageRestrictionManagerProxy>();
            _inMemoryMetricsFactory = new InMemoryMetricsFactory();
            _metrics = new MetricsImpl(_inMemoryMetricsFactory);
            _undertest = new RemoteBaggageRestrictionManager(SERVICE_NAME, _baggageRestrictionProxy, _metrics, false);
        }

        public void Dispose()
        {
            _undertest.Dispose();
        }

        [Fact]
        public void TestUpdateBaggageRestrictions()
        {
            _baggageRestrictionProxy.GetBaggageRestrictionsAsync(SERVICE_NAME).Returns(new List<BaggageRestrictionResponse> { RESTRICTION });

            _undertest.UpdateBaggageRestrictions();

            Assert.Equal(new Restriction(true, MAX_VALUE_LENGTH), _undertest.GetRestriction(SERVICE_NAME, BAGGAGE_KEY));
            Assert.False(_undertest.GetRestriction(SERVICE_NAME, "bad-key").KeyAllowed);
            Assert.True(_inMemoryMetricsFactory.GetCounter("jaeger:baggage_restrictions_updates", "result=ok") > 0L);
        }

        [Fact]
        public void TestAllowBaggageOnInitializationFailure()
        {
            _baggageRestrictionProxy.GetBaggageRestrictionsAsync(SERVICE_NAME)
                .Returns<List<BaggageRestrictionResponse>>(_ => throw new InvalidOperationException());

            _undertest = new RemoteBaggageRestrictionManager(SERVICE_NAME, _baggageRestrictionProxy, _metrics,
                false, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Assert.True(_undertest.GetRestriction(SERVICE_NAME, BAGGAGE_KEY).KeyAllowed);

            _undertest.UpdateBaggageRestrictions();

            Assert.False(_undertest.IsReady());
            // If baggage restriction update fails, all baggage should still be allowed.
            Assert.True(_undertest.GetRestriction(SERVICE_NAME, BAGGAGE_KEY).KeyAllowed);
            Assert.True(_inMemoryMetricsFactory.GetCounter("jaeger:baggage_restrictions_updates", "result=err") > 0L);
        }

        [Fact]
        public void TestDenyBaggageOnInitializationFailure()
        {
            _baggageRestrictionProxy.GetBaggageRestrictionsAsync(SERVICE_NAME)
                .Returns(new List<BaggageRestrictionResponse> { RESTRICTION });

            _undertest = new RemoteBaggageRestrictionManager(SERVICE_NAME, _baggageRestrictionProxy, _metrics,
                true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Assert.False(_undertest.GetRestriction(SERVICE_NAME, BAGGAGE_KEY).KeyAllowed);

            _undertest.UpdateBaggageRestrictions();

            Assert.True(_undertest.IsReady());
            Assert.True(_undertest.GetRestriction(SERVICE_NAME, BAGGAGE_KEY).KeyAllowed);
        }
    }
}
