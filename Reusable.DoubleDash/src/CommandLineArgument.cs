using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Reusable.Essentials.Diagnostics;
using Reusable.Essentials.Extensions;

namespace Reusable.DoubleDash;

/// <summary>
/// This class represents a single command-line argument with all its values.
/// </summary>
[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class CommandLineArgument : List<string>, IEquatable<CommandLineArgument>, IFormattable
{
    public const string DefaultFormat = "-:";

    internal CommandLineArgument(ArgumentName name, IEnumerable<string> values) : base(values) => Name = name;

    internal CommandLineArgument(ArgumentName name, params string[] values) : this(name, values.AsEnumerable()) { }

    //public static CommandLineArgument NotFound => new CommandLineArgument(MultiName.Empty, Enumerable.Empty<string>());

    private string DebuggerDisplay => ToString();

    public ArgumentName Name { get; }
        
    public static CommandLineArgument Create(string name, IEnumerable<string> values) => new CommandLineArgument(new ArgumentName(name), values);

    #region IEquatable

    public bool Equals(CommandLineArgument? other) => Name.Equals(other?.Name);

    public override bool Equals(object obj) => Equals(obj as CommandLineArgument);

    public override int GetHashCode() => Name.GetHashCode();

    #endregion

    #region IFormattable

    public string ToString(string format, IFormatProvider formatProvider)
    {
        //var match = Regex.Match(format, @"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
        //var (success, (argumentPrefix, argumentValueSeparator)) = format.Parse<string, string>(@"(?<ArgumentPrefix>[-\/\.])(?<ArgumentValueSeparator>[:= ])");
        //var (success, (argumentPrefix, argumentValueSeparator)) = format.Parse<string, string>(@"(?<T1>[-\/\.])(?<T2>[:= ])");

        // if (!success)
        // {
        //     throw new FormatException(@"Invalid command argument format. Allowed values are argument prefixes [-/.] and argument value separators [:=], e.g. '-='.");
        // }

        var argumentPrefix = "--";
        var argumentValueSeparator = "=";

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

    public static implicit operator CommandLineArgument(ArgumentName name) => new CommandLineArgument(name);
        
    public static implicit operator string(CommandLineArgument commandLineArgument) => commandLineArgument?.ToString() ?? string.Empty;

    public static implicit operator bool(CommandLineArgument arg) => arg.Name.Any();
}