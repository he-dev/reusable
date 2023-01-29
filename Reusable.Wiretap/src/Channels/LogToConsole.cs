using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public class LogToConsole : Channel
{
    public string Template { get; set; } = @"{timestamp:HH:mm:ss:fff} | {name} | {status} | {elapsed} | {details} | {attachment}";

    public IDictionary<string, IConsoleStyle> Styles { get; set; } = new Dictionary<string, IConsoleStyle>(SoftString.Comparer)
    {
        { nameof(UnitOfWorkStatus.Started), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Yellow) },
        { nameof(UnitOfWorkStatus.Running), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray) },
        { nameof(UnitOfWorkStatus.Completed), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Green) },
        { nameof(UnitOfWorkStatus.Canceled), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Magenta) },
        { nameof(UnitOfWorkStatus.Faulted), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Red) },
        { nameof(UnitOfWorkStatus.Inconclusive), new ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkCyan) },
    };

    protected override void InvokeThis(LogEntry entry)
    {
        using (Styles.TryGetValue(entry.Status, out var style) ? style.Apply() : Disposable.Empty)
        {
            Console.WriteLine(Template.Format(entry));
        }
    }
}
