using System;

namespace Jaeger.Core.Util
{
    /// <summary>
    /// <see cref="RateLimiter"/> is a rate limiter based on leaky bucket algorithm, formulated in terms of a
    /// credits balance that is replenished every time <see cref="CheckCredit"/> method is called (tick) by the amount proportional
    /// to the time elapsed since the last tick, up to max of creditsPerSecond. A call to <see cref="CheckCredit"/> takes a cost
    /// of an item we want to pay with the balance. If the balance exceeds the cost of the item, the item is "purchased"
    /// and the balance reduced, indicated by returned value of true. Otherwise the balance is unchanged and return false.
    /// <para/>
    /// This can be used to limit a rate of messages emitted by a service by instantiating the Rate Limiter with the
    /// max number of messages a service is allowed to emit per second, and calling <c>CheckCredit(1.0)</c> for each message
    /// to determine if the message is within the rate limit.
    /// <para/>
    /// It can also be used to limit the rate of traffic in bytes, by setting creditsPerSecond to desired throughput
    /// as bytes/second, and calling <see cref="CheckCredit"/> with the actual message size.
    /// </summary>
    public class RateLimiter
    {
        private readonly double _creditsPerMillisecond;
        private readonly double _maxBalance;
        private readonly IClock _clock;

        private double _balance;
        private long _lastTick;

        public RateLimiter(double creditsPerSecond, double maxBalance)
            : this(creditsPerSecond, maxBalance, new SystemClock())
        {
        }

        public RateLimiter(double creditsPerSecond, double maxBalance, IClock clock)
        {
            _creditsPerMillisecond = creditsPerSecond / 1000;
            _maxBalance = maxBalance;
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _balance = maxBalance;
            _lastTick = _clock.UtcNow().ToUnixTimeMilliseconds();
        }

        public bool CheckCredit(double itemCost)
        {
            // calculate how much time passed since the last tick, and update current tick
            long currentTime = _clock.UtcNow().ToUnixTimeMilliseconds();
            long elapsedTime = currentTime - _lastTick;
            _lastTick = currentTime;

            // calculate how much credit have we accumulated since the last tick
            _balance += elapsedTime * _creditsPerMillisecond;
            if (_balance > _maxBalance)
            {
                _balance = _maxBalance;
            }

            // if we have enough credits to pay for current item, then reduce balance and allow
            if (_balance >= itemCost)
            {
                _balance -= itemCost;
                return true;
            }
            return false;
        }
    }
}
