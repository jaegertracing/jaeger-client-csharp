using System;
using System.Net;

namespace Jaeger.Util
{

    /// <summary>
    /// A function that can return new random bytes. The function must
    /// be thread safe.
    /// </summary>
    public delegate void RandomNextBytes(byte[] bytes);

    public static class Utils
    {
        private static readonly Random Random = new Random();

        public static int IpToInt(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentNullException(nameof(ipAddress));

            if (string.Equals(ipAddress, "localhost", StringComparison.Ordinal))
            {
                return (127 << 24) | 1;
            }

            return IpToInt(IPAddress.Parse(ipAddress));
        }

        public static int IpToInt(IPAddress ipAddress)
        {
            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));

            byte[] octets = ipAddress.GetAddressBytes();
            if (octets.Length != 4)
                throw new FormatException("Not four octets");

            int intIp = 0;
            foreach (byte octet in octets)
            {
                intIp = (intIp << 8) | (octet & 0xFF);
            }
            return intIp;
        }

        /// <summary>
        /// Default implementation of RandomNextBytes that uses a static Random object
        /// </summary>
        public static void DefaultNextBytes(byte[] bytes)
        {
            lock (Random)
            {
                Random.NextBytes(bytes);
            }
        }

        public static long UniqueId(RandomNextBytes nextBytes)
        {
            if ( nextBytes == null )
            {
                nextBytes = DefaultNextBytes;
            }
            long value = 0;
            while (value == 0)
            {
                var bytes = new byte[8];
                nextBytes(bytes);
                value = BitConverter.ToInt64(bytes, 0);
            }
            return value;
        }
    }
}