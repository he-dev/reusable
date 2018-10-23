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
    public class CommandArgument : List<string>, IGrouping<Identifier, string>, IEquatable<Identifier>, IFormattable
    {
        public const string DefaultFormat = "-:";

        internal CommandArgument(Identifier key) => Key = key;

        internal CommandArgument() : this(Identifier.Empty)
        {
        }

        private string DebuggerDisplay => ToString();

        public Identifier Key { get; }

        public static CommandArgument Empty { get; } = new CommandArgument();

        #region IEquatable

        public bool Equals(Identifier other) => Key.Equals(other);

        public override bool Equals(object obj) => obj is Identifier identifier && Equals(identifier);

        public override int GetHashCode() => Key.GetHashCode();

        #endregion

        #region IFormattable

        public string ToString(string format, IFormatProvider formatProvider)
        {
            //var match = Regex.Match(format, @"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
            //var (success, (argumentPrefix, argumentValueSeparator)) = format.Parse<string, string>(@"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
            var (success, (argumentPrefix, argumentValueSeparator)) = format.Parse<string, string>(@"(?<T1>[-\/\.])(?<T2>[:= ])");

            if (!success)
            {
                throw new FormatException(@"Invalid command argument format. Allowed values are argument prefixes [-/.] and argument value separators [:=], e.g. '-='.");
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
}