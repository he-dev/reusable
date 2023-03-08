using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachLevel : IMiddleware
{
    public const string Key = "level";

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        next(entry.SetItem(Key, MapStatusToLevel(entry.Status)));
    }

    private static string MapStatusToLevel(string status)
    {
        return status switch
        {
            nameof(TaskStatus.Started) => "info",
            nameof(TaskStatus.Running) => "debug",
            nameof(TaskStatus.Completed) => "info",
            nameof(TaskStatus.Canceled) => "warning",
            nameof(TaskStatus.Faulted) => "error",
            nameof(TaskStatus.Ended) => "warning",
            _ => "warning"
        };
    }
}