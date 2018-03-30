using System;

namespace Jaeger.Core.Reporters
{
    public class ReportingConstants
    {
        public const int RemoteReporterDefaultMaxQueueSize = 100;

        public static readonly TimeSpan RemoteReporterDefaultFlushIntervalMs = TimeSpan.FromMilliseconds(100);
    }
}
