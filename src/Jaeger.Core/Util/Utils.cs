using System;
using System.Net;

namespace Jaeger.Core.Util
{
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

        public static long UniqueId()
        {
            long value = 0;
            while (value == 0)
            {
                var bytes = new byte[8];
                Random.NextBytes(bytes);
                value = BitConverter.ToInt64(bytes, 0);
            }
            return value;
        }
    }
}