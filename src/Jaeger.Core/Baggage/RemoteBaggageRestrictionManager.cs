using System;
using System.Collections.Generic;
using System.Threading;
using Jaeger.Core.Baggage.Http;
using Jaeger.Core.Metrics;

namespace Jaeger.Core.Baggage
{
    /// <summary>
    /// <see cref="RemoteBaggageRestrictionManager"/> returns a <see cref="IBaggageRestrictionManager"/>
    /// that polls the agent for the latest baggage restrictions.
    /// </summary>
    public class RemoteBaggageRestrictionManager : IBaggageRestrictionManager, IDisposable
    {
        private static readonly TimeSpan DefaultRefreshInveral = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan DefaultInitialDelay = TimeSpan.Zero;

        private readonly string _serviceName;
        private readonly IBaggageRestrictionManagerProxy _proxy;
        private readonly Timer _pollTimer;
        private readonly IMetrics _metrics;
        private readonly bool _denyBaggageOnInitializationFailure;
        private volatile bool _initialized;
        private volatile Dictionary<string, Restriction> _restrictions = new Dictionary<string, Restriction>();
        private readonly Restriction _invalidRestriction;
        private readonly Restriction _validRestriction;

        public RemoteBaggageRestrictionManager(
            string serviceName,
            IBaggageRestrictionManagerProxy proxy,
            IMetrics metrics,
            bool denyBaggageOnInitializationFailure
        )
            : this(serviceName, proxy, metrics, denyBaggageOnInitializationFailure, DefaultRefreshInveral)
        {
        }

        public RemoteBaggageRestrictionManager(
            string serviceName,
            IBaggageRestrictionManagerProxy proxy,
            IMetrics metrics,
            bool denyBaggageOnInitializationFailure,
            TimeSpan refreshInterval
        )
            : this(serviceName, proxy, metrics, denyBaggageOnInitializationFailure, refreshInterval, DefaultInitialDelay)
        {
        }

        /// <summary>
        /// Creates a <see cref="RemoteBaggageRestrictionManager"/> that fetches <see cref="BaggageRestrictionResponse"/> from a remote
        /// agent and keeps track of <see cref="Restriction"/> for a service.
        /// <para/>
        /// <paramref name="initialDelay"/> is only exposed for testing purposes so users can determine when the first call to
        /// remote agent is made. Under normal operations, this RemoteBaggageRestrictionManager will start up and
        /// asynchronously fetch restrictions. If the user wants to know if restrictions are ready, they can check via
        /// isReady().
        /// </summary>
        /// <param name="serviceName">Restrictions for this service are kept track of.</param>
        /// <param name="proxy">Proxy to remote agent.</param>
        /// <param name="metrics">Metrics for metrics emission.</param>
        /// <param name="denyBaggageOnInitializationFailure">
        /// Determines the startup failure mode of <see cref="RemoteBaggageRestrictionManager"/>.
        /// If <paramref name="denyBaggageOnInitializationFailure"/> is true,
        /// <see cref="RemoteBaggageRestrictionManager"/> will not allow any baggage to be written
        /// until baggage restrictions have been retrieved from agent. If
        /// <paramref name="denyBaggageOnInitializationFailure"/> is false,
        /// <see cref="RemoteBaggageRestrictionManager"/> will allow any baggage to be written
        /// until baggage restrictions have been retrieved from agent.
        /// </param>
        /// <param name="refreshInterval">How often restriction are fetched from remote agent.</param>
        /// <param name="initialDelay">Delay before first fetch of restrictions.</param>
        public RemoteBaggageRestrictionManager(
            string serviceName,
            IBaggageRestrictionManagerProxy proxy,
            IMetrics metrics,
            bool denyBaggageOnInitializationFailure,
            TimeSpan refreshInterval,
            TimeSpan initialDelay
        )
        {
            _serviceName = serviceName;
            _proxy = proxy;
            _metrics = metrics;
            _denyBaggageOnInitializationFailure = denyBaggageOnInitializationFailure;
            _initialized = false;
            _invalidRestriction = new Restriction(false, 0);
            _validRestriction = new Restriction(true, Restriction.DefaultMaxValueLength);

            _pollTimer = new Timer(_ => { UpdateBaggageRestrictions(); }, null, initialDelay, refreshInterval);
        }

        public bool IsReady()
        {
            return _initialized;
        }

        internal void UpdateBaggageRestrictions()
        {
            List<BaggageRestrictionResponse> response;
            try
            {
                response = _proxy.GetBaggageRestrictionsAsync(_serviceName).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // TODO swallow exception without logging it somewhere?
                _metrics.BaggageRestrictionsUpdateFailure.Inc(1);
                return;
            }

            UpdateBaggageRestrictions(response);
            _metrics.BaggageRestrictionsUpdateSuccess.Inc(1);
        }

        private void UpdateBaggageRestrictions(List<BaggageRestrictionResponse> restrictions)
        {
            if (restrictions != null)
            {
                var baggageRestrictions = new Dictionary<string, Restriction>();
                foreach (BaggageRestrictionResponse restriction in restrictions)
                {
                    baggageRestrictions[restriction.BaggageKey] = new Restriction(true, restriction.MaxValueLength);
                }
                _restrictions = baggageRestrictions;
            }
            _initialized = true;
        }

        public void Dispose()
        {
            _pollTimer.Dispose();
        }

        public Restriction GetRestriction(string service, string key)
        {
            if (!_initialized)
            {
                if (_denyBaggageOnInitializationFailure)
                {
                    return _invalidRestriction;
                }
                else
                {
                    return _validRestriction;
                }
            }

            if (_restrictions.TryGetValue(key, out Restriction restriction))
            {
                return restriction;
            }
            return _invalidRestriction;
        }
    }
}