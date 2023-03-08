using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public class LogToConsole : Channel
{
    public string Template { get; set; } = @"{timestamp:HH:mm:ss:fff} | {scope} | {status} | {elapsed} | {details} | {attachment}";

    public IDictionary<string, IConsoleStyle> Styles { get; set; } = new Dictionary<string, IConsoleStyle>(SoftString.Comparer)
    {
        { nameof(TaskStatus.Started), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Yellow) },
        { nameof(TaskStatus.Running), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray) },
        { nameof(TaskStatus.Completed), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Green) },
        { nameof(TaskStatus.Canceled), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Magenta) },
        { nameof(TaskStatus.Faulted), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Red) },
        { nameof(TaskStatus.Ended), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkCyan) },
    };

    protected override void InvokeThis(LogEntry entry)
    {
        using (Styles.TryGetValue(entry.Status, out var style) ? style.Apply() : new Disposable.Empty())
        {
            Console.WriteLine(Template.Format((string name, out object value) => entry.TryGetValue(name, out value)));
        }
    }
}
