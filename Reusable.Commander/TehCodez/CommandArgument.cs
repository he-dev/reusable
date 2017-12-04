using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.Collections;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    public class CommandArgument : List<string>, IGrouping<SoftKeySet, string>, IEquatable<SoftKeySet>
    {
        public static readonly SoftKeySet CommandNameKey = SoftKeySet.Empty;

        internal CommandArgument(SoftKeySet key) => Key = key;

        public SoftKeySet Key { get; }

        #region IEquatable

        public bool Equals(SoftKeySet other) => Key.Equals(other);

        public override bool Equals(object obj) => obj is SoftKeySet nameSet && Equals(nameSet);

        public override int GetHashCode() => Key.GetHashCode();

        #endregion

        public override string ToString()
        {
            // TODO implement this as a custom formatter

            var format = "-=";

            var match = Regex.Match(format, @"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");

            //            if (!match.Success)
            //            {
            //                throw new FormatException(@"Invalid format. Expected argument prefix: [-/.], argument value separator: [:=]");
            //            }

            var result = new StringBuilder();

            result.Append(
                string.IsNullOrEmpty(Key.FirstOrDefault()?.ToString())
                    ? string.Empty
                    : $"{match.Groups["ArgumentPrefix"].Value}{Key.FirstOrDefault()}");

            result.Append(
                result.Length > 0 && this.Any()
                    ? match.Groups["ArgumentValueSeparator"].Value
                    : string.Empty);

            result.Append(string.Join(", ", this.Select(x => x.Contains(' ') ? $"\"{x}\"" : x)));

            return result.ToString();
        }

        public static implicit operator string(CommandArgument commandArgument) => commandArgument?.ToString();
    }
}