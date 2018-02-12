using System;

namespace LetsTrace.Util
{
    public class RandomGenerator
    {
        public static readonly Random random = new Random();

        public static UInt64 RandomId()
        {
            var bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}