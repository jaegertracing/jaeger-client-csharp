using System;
using System.Collections.Generic;

namespace Jaeger.Core
{
    public class LogRecord
    {
        public DateTime TimestampUtc { get; }
        public IDictionary<string, object> Fields { get; }

        public LogRecord(DateTime timestampUtc, IDictionary<string, object> fields)
        {
            TimestampUtc = timestampUtc;
            Fields = fields;
        }
    }
}