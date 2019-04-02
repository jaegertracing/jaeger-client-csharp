using System;
using System.Globalization;
using Jaeger.Util;

namespace Jaeger
{
    /// <summary>
    /// Represents a unique 128bit identifier of a trace.
    /// </summary>
    public readonly struct TraceId
    {
        public long High { get; }
        public long Low { get; }
        public bool IsZero => Low == 0 && High == 0;

        public static TraceId NewUniqueId(bool useHigh)
        {
            var high = useHigh ? Utils.UniqueId() : 0;
            var low = Utils.UniqueId();
            return new TraceId(high, low);
        }

        public TraceId(long low) : this(0, low)
        {
        }

        public TraceId(long high, long low)
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

        public byte[] ToByteArray()
        {
            var bytesHigh = Utils.LongToNetworkBytes(High);
            var bytesLow = Utils.LongToNetworkBytes(Low);

            var bytes = new byte[bytesHigh.Length + bytesLow.Length];
            Array.Copy(bytesHigh, 0, bytes, 0, bytesHigh.Length);
            Array.Copy(bytesLow, 0, bytes, bytesHigh.Length, bytesLow.Length);

            return bytes;
        }

        public static TraceId FromString(string from)
        {
            if (from.Length > 32)
            {
                throw new Exception($"TraceId cannot be longer than 32 hex characters: {from}");
            }

            long high = 0, low = 0;

            if (from.Length > 16)
            {
                var highLength = from.Length - 16;
                var highString = from.Substring(0, highLength);
                var lowString = from.Substring(highLength);

                if (!long.TryParse(highString, NumberStyles.HexNumber, null, out high))
                {
                    throw new Exception($"Cannot parse High TraceId from string: {highString}");
                }

                if (!long.TryParse(lowString, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {lowString}");
                }
            }
            else
            {
                if (!long.TryParse(from, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {from}");
                }
            }

            return new TraceId(high, low);
        }
    }
}