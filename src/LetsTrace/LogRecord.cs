using System;
using System.Collections.Generic;

namespace LetsTrace
{
    public class LogRecord
    {
        public DateTime TimestampUtc { get; }
        public List<Field> Fields { get; }

        public LogRecord(DateTime timestampUtc, List<Field> fields)
        {
            TimestampUtc = timestampUtc;
            Fields = fields;
        }
    }
}