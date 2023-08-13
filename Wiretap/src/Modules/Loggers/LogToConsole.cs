using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules.Loggers;

[PublicAPI]
public class LogToConsole : ILog
{
    public string Template { get; set; } = @"{timestamp:HH:mm:ss:fff} | {activity} | {trace} | {elapsed} | {message} | {details} | {attachment}";

    public IDictionary<string, IConsoleStyle> Styles { get; set; } = new Dictionary<string, IConsoleStyle>(SoftString.Comparer)
    {
        { Strings.Traces.Begin, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Yellow) },
        { Strings.Traces.Info, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray) },
        { Strings.Traces.Noop, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkGreen) },
        { Strings.Traces.Break, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Magenta) },
        { Strings.Traces.End, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Green) },
        { Strings.Traces.Error, new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Red) },
    };

    public void Invoke(TraceContext context, LogFunc next)
    {
        if (context.Entry.GetItem<string>(Strings.Items.Trace) is { } styleName)
        {
            using (Styles.TryGetValue(styleName, out var style) ? style.Apply() : new Disposable.Empty())
            {
                Console.WriteLine(Template.Format((string name, out object? value) => context.Entry.TryGetValue(name, out value)));
            }
        }

        next(context);
    }
}
