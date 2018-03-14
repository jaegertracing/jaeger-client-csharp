using System;

namespace LetsTrace.Reporters
{
    public class ReportingConstants
    {
        public const int REMOTE_REPORTER_DEFAULT_MAX_QUEUE_SIZE = 100;

        public static readonly TimeSpan REMOTE_REPORTER_DEFAULT_FLUSH_INTERVAL_MS = TimeSpan.FromMilliseconds(100);
    }
}
