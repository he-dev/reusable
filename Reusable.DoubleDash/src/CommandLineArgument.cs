using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Reusable.DoubleDash;

public class CommandLineArgument
{
    public string Name { get; set; }

    public IList<string> Values { get; } = new List<string>();
    

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