namespace LetsTrace.Util
{
    // IRateLimiter is a filter used to check if a message that is worth itemCost units is within the rate limits.
    public interface IRateLimiter
    {
        bool CheckCredit(double itemCost);
    }
}