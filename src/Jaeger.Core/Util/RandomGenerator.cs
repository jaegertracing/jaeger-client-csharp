using System;

namespace Jaeger.Core.Util
{
    public class RandomGenerator
    {
        private static readonly Random Random = new Random();

        public static ulong RandomId()
        {
            var bytes = new byte[8];
            Random.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}