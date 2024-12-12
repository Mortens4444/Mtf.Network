using System;
using System.Text;

namespace Mtf.Network.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToZeroByteTerminatedString(this byte[] value, Encoding encoding)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var length = Array.IndexOf(value, (byte)0);
            length = length == -1 ? value.Length : length;
            return encoding.GetString(value, 0, length);
        }
    }
}
