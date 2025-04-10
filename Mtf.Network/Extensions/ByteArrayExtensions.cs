using System;
using System.Linq;
using System.Text;

namespace Mtf.Network.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] AppendArrays(this byte[] first, params byte[][] arrays)
        {
            var totalLength = first.Length + arrays.Sum(a => a.Length);
            var result = new byte[totalLength];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);

            var offset = first.Length;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

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
