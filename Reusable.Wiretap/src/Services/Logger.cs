using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services;

public class Logger : ILogger
{
    public Logger(Action<LogEntry> next) => Next = next;

    private Action<LogEntry> Next { get; }

    public void Log(LogEntry entry) => Next(entry);

    public static readonly ILogger Empty = new Logger(_ => { });
}