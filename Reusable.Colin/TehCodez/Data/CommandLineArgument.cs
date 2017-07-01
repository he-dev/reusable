using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Data
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    public class CommandLineArgument : List<string>, IGrouping<IImmutableNameSet, string>, IEquatable<IImmutableNameSet>
    {
        internal CommandLineArgument(IImmutableNameSet key) => Key = key;

        public IImmutableNameSet Key { get; }

        public bool Equals(IImmutableNameSet other)
        {
            return ImmutableNameSet.Comparer.Equals(Key, other);
        }

        public override bool Equals(object obj)
        {
            return obj is CommandLineArgument cmdArg && Equals(cmdArg.Key);
        }

        public override int GetHashCode()
        {
            return ImmutableNameSet.Comparer.GetHashCode(Key);
        }
    }

    public static class CommandLineArgumentExtensions
    {
        public static string ToCommandLine(this IGrouping<IImmutableNameSet, string> argument, string format)
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