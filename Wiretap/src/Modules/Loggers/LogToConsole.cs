using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules.Loggers;

[PublicAPI]
public class LogToConsole : ILog
{
    public string Template { get; set; } = @"{timestamp:HH:mm:ss:fff} | {activity} | {trace} | {elapsed} | {message} | {details} | {attachment}";

    public IDictionary<string, IConsoleStyle> Styles { get; set; } = new Dictionary<string, IConsoleStyle>(SoftString.Comparer)
    {
        { nameof(ActivityTraces.LogBegin), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Yellow) },
        { nameof(ActivityTraces.LogInfo), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray) },
        { nameof(ActivityTraces.LogNoop), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkGreen) },
        { nameof(ActivityTraces.LogStop), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Magenta) },
        { nameof(ActivityTraces.LogEnd), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Green) },
        { nameof(ActivityTraces.LogError), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Red) },
    };

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (entry.TryGetValue(LogEntry.PropertyNames.Trace, out var trace) && trace is string styleName)
        {
            using (Styles.TryGetValue(styleName, out var style) ? style.Apply() : new Disposable.Empty())
            {
                Console.WriteLine(Template.Format((string name, out object? value) => entry.TryGetValue(name, out value)));
            }
        }

        next(activity, entry);
    }
}
