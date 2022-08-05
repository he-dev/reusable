using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Reusable.Marbles.Diagnostics;
using Reusable.Marbles.Extensions;

namespace Reusable.DoubleDash;

/// <summary>
/// This class represents a single command-line argument with all its values.
/// </summary>
[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class CommandLineArgument : List<string>, IEquatable<CommandLineArgument>, IFormattable
{
    public const string DefaultFormat = "-:";

    internal CommandLineArgument(NameCollection nameCollection, IEnumerable<string> values) : base(values) => NameCollection = nameCollection;

    internal CommandLineArgument(NameCollection nameCollection, params string[] values) : this(nameCollection, values.AsEnumerable()) { }

    //public static CommandLineArgument NotFound => new CommandLineArgument(MultiName.Empty, Enumerable.Empty<string>());

    private string DebuggerDisplay => ToString();

    public NameCollection NameCollection { get; }
        
    public static CommandLineArgument Create(string name, IEnumerable<string> values) => new CommandLineArgument(new NameCollection(name), values);

    #region IEquatable

    public bool Equals(CommandLineArgument? other) => NameCollection.Equals(other?.NameCollection);

    public override bool Equals(object obj) => Equals(obj as CommandLineArgument);

    public override int GetHashCode() => NameCollection.GetHashCode();

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
            string.IsNullOrEmpty(NameCollection.FirstOrDefault()?.ToString())
                ? string.Empty
                : $"{argumentPrefix}{NameCollection.FirstOrDefault()}");

        result.Append(
            result.Any() && this.Any()
                ? argumentValueSeparator
                : string.Empty);

        result.Append(string.Join(", ", this.Select(x => x.Contains(' ') ? $"\"{x}\"" : x)));

        return result.ToString();
    }

    #endregion

    public override string ToString() => ToString(DefaultFormat, CultureInfo.InvariantCulture);

    public static implicit operator CommandLineArgument(NameCollection nameCollection) => new CommandLineArgument(nameCollection);
        
    public static implicit operator string(CommandLineArgument commandLineArgument) => commandLineArgument?.ToString() ?? string.Empty;

    public static implicit operator bool(CommandLineArgument arg) => arg.NameCollection.Any();
}