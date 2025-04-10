using System;
using System.Collections.Generic;
using System.IO;

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

        public static string Remove(this string value, string removable)
        {
            return value.Replace(removable, String.Empty);
        }

        public static string[] GetProgramAndParameters(this string command)
        {
            string parameter;
            var result = new List<string>();

            var i = 0;
            while (i < command.Length)
            {
                switch (command[i])
                {
                    case '"':
                        var nextQuotIndex = command.IndexOf('"', i + 1);
                        if (nextQuotIndex == -1)
                        {
                            throw new InvalidDataException("Could not find closing quotation mark");
                        }
                        parameter = command.Substring(i + 1, nextQuotIndex - i - 1);
                        result.Add(parameter);
                        i = nextQuotIndex;
                        break;
                    case ' ':
                        break;
                    default:
                        var nextParamEndIndex = command.IndexOf(' ', i + 1);
                        parameter = nextParamEndIndex == -1 ? command.Substring(i) : command.Substring(i, nextParamEndIndex - i);
                        result.Add(parameter);
                        i = nextParamEndIndex == -1 ? command.Length : nextParamEndIndex;
                        break;
                }
                i++;
            }
            return result.ToArray();
        }
    }
}
