using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Data
{
    public static class CommandLineArgumentExtensions
    {
        public static string ToCommandLineString(this IGrouping<IImmutableNameSet, string> argument, string format)
        {
            var match = Regex.Match(format, @"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
            if (!match.Success) { throw new FormatException(@"Invalid format. Expected argument prefix: [-/.], argument value separator: [:=]"); }

            var result = new StringBuilder();

            result.Append(
                string.IsNullOrEmpty(argument.Key.FirstOrDefault())
                    ? string.Empty
                    : $"{match.Groups["ArgumentPrefix"].Value}{argument.Key.FirstOrDefault()}");

            result.Append(
                result.Length > 0 && argument.Any()
                    ? match.Groups["ArgumentValueSeparator"].Value
                    : string.Empty);

            result.Append(string.Join(", ", argument.Select(x => x.Contains(' ') ? $"\"{x}\"" : x)));

            return result.ToString();
        }
    }
}