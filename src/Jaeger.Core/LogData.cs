using System;
using System.Collections.Generic;

namespace Jaeger.Core
{
    public sealed class LogData
    {
        public DateTime TimestampUtc { get; }
        public string Message { get; }
        public IDictionary<string, object> Fields { get; }

        public LogData(DateTime timestampUtc, string message)
        {
            TimestampUtc = timestampUtc;
            Message = message;
        }

        public LogData(DateTime timestampUtc, IDictionary<string, object> fields)
        {
            TimestampUtc = timestampUtc;
            Fields = fields;
        }
    }
}