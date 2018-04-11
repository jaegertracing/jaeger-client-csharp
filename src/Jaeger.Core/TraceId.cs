using System;
using System.Globalization;

namespace Jaeger.Core
{
    /// <summary>
    /// Represents a unique 128bit identifier of a trace.
    /// </summary>
    public class TraceId
    {
        public ulong High { get; }
        public ulong Low { get; }
        public bool IsValid => High != 0 || Low != 0;

        public TraceId(ulong low) : this(0, low)
        {
        }

        public TraceId(ulong high, ulong low)
        {
            High = high;
            Low = low;
        }

        public override string ToString()
        {
            if (High == 0)
            {
                return Low.ToString("x");
            }

            return $"{High:x}{Low:x016}";
        }

        public static TraceId FromString(string from)
        {
            if (from.Length > 32)
            {
                throw new Exception($"TraceId cannot be longer than 32 hex characters: {from}");
            }

            ulong high = 0, low = 0;

            if (from.Length > 16)
            {
                var highLength = from.Length - 16;
                var highString = from.Substring(0, highLength);
                var lowString = from.Substring(highLength);

                if (!ulong.TryParse(highString, NumberStyles.HexNumber, null, out high))
                {
                    throw new Exception($"Cannot parse High TraceId from string: {highString}");
                }

                if (!ulong.TryParse(lowString, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {lowString}");
                }
            }
            else
            {
                if (!ulong.TryParse(from, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {from}");
                }
            }

            return new TraceId(high, low);
        }
    }
}