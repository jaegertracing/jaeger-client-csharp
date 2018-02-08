using System;
using System.Collections.Generic;

namespace LetsTrace
{
    public class LogRecord
    {
        public readonly DateTimeOffset Timestamp;
        public readonly string Message;
        public readonly IEnumerable<KeyValuePair<string, object>> Fields;

        public LogRecord(DateTimeOffset timestamp, string message, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Message = message;
            Fields = fields;
        }
    }
}