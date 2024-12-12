using System;

namespace Mtf.Network.Extensions
{
    public static class StringExtensions
    {
        public static string ExtractBetween(this string value, string startDelimiter, string endDelimiter, StringComparison stringComparison = StringComparison.Ordinal, int initialStartIndex = 0)
        {
            if (String.IsNullOrEmpty(startDelimiter))
            {
                throw new ArgumentException("Parameter should not be null or empty.", nameof(startDelimiter));
            }

            if (!String.IsNullOrEmpty(value))
            {
                var startIndex = value.IndexOf(startDelimiter, initialStartIndex, stringComparison);
                if (startIndex != -1)
                {
                    startIndex += startDelimiter.Length;
                    var endIndex = value.IndexOf(endDelimiter, startIndex, stringComparison);
                    if (endIndex > startIndex)
                    {
                        return value.Substring(startIndex, endIndex - startIndex);
                    }
                }
            }
            return null;
        }
    }
}
