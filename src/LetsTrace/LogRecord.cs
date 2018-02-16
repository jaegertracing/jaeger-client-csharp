using System;
using System.Collections.Generic;

namespace LetsTrace
{
    public class LogRecord
    {
        public DateTimeOffset Timestamp { get; }
        public List<Field> Fields { get; }

        public LogRecord(DateTimeOffset timestamp, List<Field> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }
    }
}