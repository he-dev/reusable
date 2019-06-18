using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Commander
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandParameter : List<string>, IEquatable<CommandParameter>, IFormattable
    {
        public const string DefaultFormat = "-:";

        internal CommandParameter(Identifier key) => Id = key;

        internal CommandParameter() : this(Identifier.Empty) { }

        private string DebuggerDisplay => ToString();

        public Identifier Id { get; }

        public static CommandParameter Empty { get; } = new CommandParameter();

        #region IEquatable

        public bool Equals(CommandParameter other) => Id?.Equals(other.Id) == true;

        public override bool Equals(object obj) => obj is CommandParameter parameter && Equals(parameter);

        public override int GetHashCode() => Id.GetHashCode();

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
                string.IsNullOrEmpty(Id.FirstOrDefault()?.ToString())
                    ? string.Empty
                    : $"{argumentPrefix}{Id.FirstOrDefault()}");

            result.Append(
                result.Any() && this.Any()
                    ? argumentValueSeparator
                    : string.Empty);

            result.Append(string.Join(", ", this.Select(x => x.Contains(' ') ? $"\"{x}\"" : x)));

            return result.ToString();
        }

        #endregion

        public override string ToString() => ToString(DefaultFormat, CultureInfo.InvariantCulture);

        public static implicit operator string(CommandParameter commandParameter) => commandParameter?.ToString();
    }
}