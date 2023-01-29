using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachLevel : IMiddleware
{
    public const string Key = "level";

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        next(entry.SetItem(Key, MapStatusToLevel(entry.Status)));
    }

    private static string MapStatusToLevel(string status)
    {
        return status switch
        {
            nameof(UnitOfWorkStatus.Started) => "i",
            nameof(UnitOfWorkStatus.Running) => "d",
            nameof(UnitOfWorkStatus.Completed) => "i",
            nameof(UnitOfWorkStatus.Canceled) => "w",
            nameof(UnitOfWorkStatus.Faulted) => "e",
            _ => "w"
        };
    }
}