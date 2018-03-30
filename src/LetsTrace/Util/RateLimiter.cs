namespace Jaeger.Core.Util
{
    // RateLimiter is a rate limiter based on leaky bucket algorithm, formulated in terms of a
    // credits balance that is replenished every time CheckCredit() method is called (tick) by the amount proportional
    // to the time elapsed since the last tick, up to max of creditsPerSecond. A call to CheckCredit() takes a cost
    // of an item we want to pay with the balance. If the balance exceeds the cost of the item, the item is "purchased"
    // and the balance reduced, indicated by returned value of true. Otherwise the balance is unchanged and return false.
    //
    // This can be used to limit a rate of messages emitted by a service by instantiating the Rate Limiter with the
    // max number of messages a service is allowed to emit per second, and calling CheckCredit(1.0) for each message
    // to determine if the message is within the rate limit.
    //
    // It can also be used to limit the rate of traffic in bytes, by setting creditsPerSecond to desired throughput
    // as bytes/second, and calling CheckCredit() with the actual message size.
    public class RateLimiter : IRateLimiter
    {
        private readonly double _creditsPerMillisecond;
        private readonly double _maxBalance;
        private readonly IClock _clock;

        private double _balance;
        private long _lastTick;

        public RateLimiter(double creditsPerSecond, double maxBalance)
            : this(creditsPerSecond, maxBalance, new Clock())
        {}

        public RateLimiter(double creditsPerSecond, double maxBalance, IClock clock)
        {
            _creditsPerMillisecond = creditsPerSecond / 1000;
            _maxBalance = maxBalance;
            _clock = clock;

            _balance = maxBalance;
            _lastTick = _clock.UtcNow().ToUnixTimeMilliseconds();
        }

        public bool CheckCredit(double itemCost)
        {
            // calculate how much time passed since the last tick, and update current tick
            var currentTime = _clock.UtcNow().ToUnixTimeMilliseconds();
            var elapsedTime = currentTime - _lastTick;
            _lastTick = currentTime;

            // calculate how much credit have we accumulated since the last tick
            _balance += elapsedTime * _creditsPerMillisecond;
            if (_balance > _maxBalance) {
                _balance = _maxBalance;
            }

            // if we have enough credits to pay for current item, then reduce balance and allow
            if (_balance >= itemCost) {
                _balance -= itemCost;
                return true;
            }
            return false;
        }
    }
}
