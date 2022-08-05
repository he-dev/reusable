using System;
using JetBrains.Annotations;
using Reusable.Marbles.Extensions;
using Reusable.Marbles.FormatProviders;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Channels;

public class ConsoleChannelDynamic : ConsoleChannel<string>
{
    public ConsoleChannelDynamic(string? name = default) : base(name) => Template = new TextMessageBuilder();

    protected override void Log(ILogEntry entry)
    {
        var level = entry[LogProperty.Names.Level()].Value switch { ConsoleStyle.LogLevel l => l, _ => ConsoleStyle.LogLevel.Information };

        using (Styles[level.ToString(), entry].Apply())
        {
            Console.WriteLine(Template.Build(entry));
        }

        Next?.Invoke(entry);
    }
}

[PublicAPI]
public class TextMessageBuilder : IConsoleMessageBuilder<string>
{
    public string Template { get; set; } = @"{Timestamp:HH:mm:ss:fff} | {Level} | {Logger} | {Message}";

    public IFormatProvider FormatProvider { get; set; } = new CompositeFormatProvider
    {
        new CaseFormatProvider(),
        new PunctuationFormatProvider(),
        new TypeFormatProvider()
    };

    public string Build(ILogEntry entry) => Template.Format(entry, FormatProvider);
}