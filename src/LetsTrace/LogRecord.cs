using System;
using System.Collections.Generic;

namespace LetsTrace
{
    public class LogRecord
    {
        public DateTimeOffset Timestamp { get; }
        public string Message { get; }
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogRecord(DateTimeOffset timestamp, string message, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Message = message;
            Fields = fields;
        }
    }
}