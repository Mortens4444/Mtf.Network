using System;

namespace Mtf.Network.Services
{
    public static class BufferHelper
    {
        public static int GetNextInt(byte[] buffer, ref int start)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (start < 0 || start >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            const int size = sizeof(int);
            if (start + size > buffer.Length)
            {
                throw new ArgumentException("Buffer does not contain enough data for the requested int.");
            }

            var result = 0;
            for (var i = size - 1; i >= 0; i--)
            {
                result = (result << 8) | buffer[start + i];
            }

            start += size;
            return result;
        }

        public static long GetNextLong(byte[] buffer, ref int start)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (start < 0 || start >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            const int size = sizeof(long);
            if (start + size > buffer.Length)
            {
                throw new ArgumentException("Buffer does not contain enough data for the requested long.");
            }

            long result = 0;
            for (var i = size - 1; i >= 0; i--)
            {
                result = (result << 8) | buffer[start + i];
            }

            start += size;
            return result;
        }
    }
}
