using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Connectors.Extensions;

namespace Reusable.Wiretap.Connectors;

public record struct ConsoleStyle(ConsoleColor ForegroundColor, ConsoleColor BackgroundColor)
{
    public static ConsoleStyle Default { get; set; } = new(ConsoleColor.Black, ConsoleColor.Gray);

    public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);
}

public interface IBuilder<out T>
{
    T Build(ILogEntry entry);
}

public abstract class ConsoleConnector<T> : IConnector
{
    public IBuilder<T> Template { get; set; } = null!;

    public IBuilder<ConsoleStyle> Style { get; set; } = null!;

    public abstract void Log(ILogEntry entry);
}

public class ConsoleConnectorDynamic : ConsoleConnector<string>
{
    protected ConsoleConnectorDynamic()
    {
        Template = new ConstantTemplate(@"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Logger}: {Message}");
        Style = new LogLevelStyle();
    }

    public IFormatProvider FormatProvider { get; set; } = new CompositeFormatProvider
    {
        new CaseFormatProvider(),
        new PunctuationFormatProvider(),
        new TypeFormatProvider()
    };

    public override void Log(ILogEntry entry)
    {
        using (Style.Build(entry).Apply())
        {
            Console.WriteLine(RenderText(entry));
        }
    }

    private string RenderText(ILogEntry entry) => Template.Build(entry).Format((string name, out object? value) =>
    {
        if (entry.TryGetProperty(name, out var property))
        {
            value = property.Value;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }, FormatProvider);
}

public class ConstantTemplate : IBuilder<string>
{
    private readonly string _value;

    public ConstantTemplate(string value) => _value = value;

    public string Build(ILogEntry entry) => _value;
}

public class LogLevelStyle : IBuilder<ConsoleStyle>
{
    public IDictionary<string, ConsoleStyle> LogLevelColor { get; set; } = new Dictionary<string, ConsoleStyle>(SoftString.Comparer)
    {
        ["Trace"] = new(ConsoleColor.DarkGray, ConsoleStyle.Default.BackgroundColor),
        ["Debug"] = new(ConsoleColor.DarkGray, ConsoleStyle.Default.BackgroundColor),
        ["Information"] = new(ConsoleColor.White, ConsoleStyle.Default.BackgroundColor),
        ["Warning"] = new(ConsoleColor.Yellow, ConsoleStyle.Default.BackgroundColor),
        ["Error"] = new(ConsoleColor.Red, ConsoleStyle.Default.BackgroundColor),
        ["Fatal"] = new(ConsoleColor.Red, ConsoleStyle.Default.BackgroundColor),
    };

    public ConsoleStyle Build(ILogEntry entry)
    {
        if (entry.TryGetProperty("Level", out var property) && LogLevelColor.TryGetValue(property.Value.ToString() ?? "Information", out var consoleStyle))
        {
            return consoleStyle;
        }

        return ConsoleStyle.Default;
    }
}