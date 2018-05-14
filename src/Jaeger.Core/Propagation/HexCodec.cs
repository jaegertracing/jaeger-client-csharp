using System;

namespace Jaeger.Core.Propagation
{
    // copy/pasted from brave.internal.HexCodec 4.1.1 to avoid build complexity
    internal static class HexCodec
    {
        internal static readonly char[] HEX_DIGITS = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

        /// <summary>
        /// Parses a 1 to 32 character lower-hex string with no prefix into an unsigned long, tossing any
        /// bits higher than 64.
        /// </summary>
        /// <returns>A 64 bit long, meaning that negative values are the overflow of Java's 32 bit long.</returns>
        internal static long? LowerHexToUnsignedLong(string lowerHex)
        {
            int length = lowerHex.Length;
            if (length < 1 || length > 32)
            {
                return null;
            }

            // trim off any high bits
            int beginIndex = length > 16 ? length - 16 : 0;

            return LowerHexToUnsignedLong(lowerHex, beginIndex);
        }

        /// <summary>
        /// Parses a 16 character lower-hex string with no prefix into an unsigned long, starting at the
        /// spe index.
        /// </summary>
        /// <returns>A 64 bit long, meaning that negative values are the overflow of C#'s 32 bit long.</returns>
        internal static long? LowerHexToUnsignedLong(string lowerHex, int index)
        {
            // TODO compiler warning. do we have to change this?
#pragma warning disable CS0675
            long result = 0;
            for (int endIndex = Math.Min(index + 16, lowerHex.Length); index < endIndex; index++)
            {
                char c = lowerHex[index];
                result <<= 4;
                if (c >= '0' && c <= '9')
                {
                    result |= c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    result |= c - 'a' + 10;
                }
                else
                {
                    return null;
                }
            }
            return result;
#pragma warning restore CS0675
        }

        /// <summary>
        /// Returns 16 or 32 character hex string depending on if <paramref name="high"/> is zero.
        /// </summary>
        internal static string ToLowerHex(long high, long low)
        {
            char[] result = new char[high != 0 ? 32 : 16];
            int pos = 0;
            if (high != 0)
            {
                WriteHexLong(result, pos, high);
                pos += 16;
            }
            WriteHexLong(result, pos, low);
            return new string(result);
        }

        /// <summary>
        /// Inspired by "okio.Buffer.writeLong" (Java).
        /// </summary>
        internal static string ToLowerHex(long v)
        {
            char[] data = new char[16];
            WriteHexLong(data, 0, v);
            return new string(data);
        }

        /// <summary>
        /// Inspired by "okio.Buffer.writeLong" (Java).
        /// </summary>
        private static void WriteHexLong(char[] data, int pos, long v)
        {
            WriteHexByte(data, pos + 0, (byte)((v >> 56) & 0xff));
            WriteHexByte(data, pos + 2, (byte)((v >> 48) & 0xff));
            WriteHexByte(data, pos + 4, (byte)((v >> 40) & 0xff));
            WriteHexByte(data, pos + 6, (byte)((v >> 32) & 0xff));
            WriteHexByte(data, pos + 8, (byte)((v >> 24) & 0xff));
            WriteHexByte(data, pos + 10, (byte)((v >> 16) & 0xff));
            WriteHexByte(data, pos + 12, (byte)((v >> 8) & 0xff));
            WriteHexByte(data, pos + 14, (byte)(v & 0xff));
        }

        private static void WriteHexByte(char[] data, int pos, byte b)
        {
            data[pos + 0] = HEX_DIGITS[(b >> 4) & 0xf];
            data[pos + 1] = HEX_DIGITS[b & 0xf];
        }
    }
}