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
    public class CommandArgument : List<string>, IEquatable<CommandArgument>, IFormattable
    {
        public const string DefaultFormat = "-:";

        internal CommandArgument(Identifier name, IEnumerable<string> values) : base(values)
        {
            Name = name;
        }

        private string DebuggerDisplay => ToString();

        public Identifier Name { get; }

        #region IEquatable

        public bool Equals(CommandArgument other) => Name?.Equals(other.Name) == true;

        public override bool Equals(object obj) => obj is CommandArgument parameter && Equals(parameter);

        public override int GetHashCode() => Name.GetHashCode();

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
                string.IsNullOrEmpty(Name.FirstOrDefault()?.ToString())
                    ? string.Empty
                    : $"{argumentPrefix}{Name.FirstOrDefault()}");

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
    
//    public class CommandArgumentComparer : IComparer<CommandArgument>
//    {
//        public int Compare(CommandArgument x, CommandArgument y)
//        {
//            if (ReferenceEquals(x, y)) return 0;
//            if (ReferenceEquals(x, null)) return -1;
//            if (ReferenceEquals(y, null)) return 1;
//
//            if (x.Name.Default.Option == y.Name.Default.Option) return 0;
//            if (x.Name.Default.Option == NameOption.CommandLine) return 1;
//            if (y.Name.Default.Option == NameOption.CommandLine) return -1;
//        }
//    }
}