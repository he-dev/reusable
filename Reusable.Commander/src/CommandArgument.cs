using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.Commander
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandArgument : List<string>, IGrouping<SoftKeySet, string>, IEquatable<SoftKeySet>, IFormattable
    {
        public const string DefaultFormat = "-:";

        internal CommandArgument(SoftKeySet key) => Key = key;

        private string DebuggerDisplay => ToString();

        public SoftKeySet Key { get; }

        public static CommandArgument Empty { get; } = new CommandArgument(SoftString.Empty);

        #region IEquatable

        public bool Equals(SoftKeySet other) => Key.Equals(other);

        public override bool Equals(object obj) => obj is SoftKeySet nameSet && Equals(nameSet);

        public override int GetHashCode() => Key.GetHashCode();

        #endregion

        #region IFormattable

        public string ToString(string format, IFormatProvider formatProvider)
        {
            //var match = Regex.Match(format, @"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
            var (success, (argumentPrefix, argumentValueSeparator)) = format.Parse<string, string>(@"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");

            if (!success)
            {
                throw new FormatException(@"Invalid format. Allowed values are argument prefixes [-/.] and argument value separators [:=], e.g. '-='.");
            }

            var result = new StringBuilder();

            result.Append(
                string.IsNullOrEmpty(Key.FirstOrDefault()?.ToString())
                    ? string.Empty
                    : $"{argumentPrefix}{Key.FirstOrDefault()}");

            result.Append(
                result.Any() && this.Any()
                    ? argumentValueSeparator
                    : string.Empty);

            result.Append(string.Join(", ", this.Select(x => x.Contains(' ') ? $"\"{x}\"" : x)));

            return result.ToString();
        }

        #endregion

        public override string ToString() => ToString(DefaultFormat, CultureInfo.InvariantCulture);

        public static implicit operator string(CommandArgument commandArgument) => commandArgument?.ToString();
    }

    public static class CommandArgumentKeys
    {
        public static readonly SoftKeySet Anonymous = SoftString.Empty;
    }
}