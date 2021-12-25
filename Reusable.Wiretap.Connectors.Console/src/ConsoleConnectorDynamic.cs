using System;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Essentials.FormatProviders;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Connectors;

public class ConsoleConnectorDynamic : ConsoleConnector<string>
{
    public ConsoleConnectorDynamic() => Template = new TextMessageBuilder();

    public override void Log(ILogEntry entry)
    {
        var level = entry.GetPropertyOrDefault<LoggableProperty.Level>().ValueOrDefault(LogLevel.Information).ToString();

        using (Styles[level, entry].Apply())
        {
            Console.WriteLine(Template.Build(entry));
        }
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