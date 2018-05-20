using System;
using System.Collections.Generic;
using System.Linq;

namespace Jaeger
{
    public sealed class LogData
    {
        public DateTime TimestampUtc { get; }
        public string Message { get; }
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogData(DateTime timestampUtc, string message)
        {
            TimestampUtc = timestampUtc;
            Message = message;
        }

        public LogData(DateTime timestampUtc, IEnumerable<KeyValuePair<string, object>> fields)
        {
            TimestampUtc = timestampUtc;
            Fields = fields;
        }

        // for testing
        internal object GetFieldValue(string key)
        {
            // We don't know for sure that we have a dictionary
            // so we have to do the "last wins" logic ourselves.
            return Fields?.LastOrDefault(x => x.Key == key).Value;
        }
    }
}