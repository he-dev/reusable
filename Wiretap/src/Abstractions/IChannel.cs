using JetBrains.Annotations;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Abstractions;

public interface IChannel : IMiddleware
{
    ILogEntryFilter Filter { get; set; }
}

[PublicAPI]
public abstract class Channel : IChannel
{
    public ILogEntryFilter Filter { get; set; } = new OptFilter();

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        if (Filter.CanLog(this, entry))
        {
            InvokeThis(entry);
        }

        next(entry);
    }

    protected abstract void InvokeThis(LogEntry entry);
}

public interface ILogEntryFilter
{
    bool CanLog(IChannel channel, LogEntry entry);
}