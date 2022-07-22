using System;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;


namespace Reusable.Wiretap.Abstractions;

public interface IChannel : ILoggerMiddleware
{
    string? Name { get; }

    IChannelFilter Filter { get; set; }
}

public abstract class Channel : LoggerMiddleware, IChannel
{
    public string? Name { get; set; }

    public IChannelFilter Filter { get; set; } = new ChannelFilter.Empty();
}

public abstract class Channel<T> : Channel where T : IChannel
{
    public override void Invoke(ILogEntry entry)
    {
        entry.Push<IMetaProperty>(LogProperty.Names.ChannelName(), Name);
        if (Filter.CanLog<T>(entry)) Log(entry);
    }

    protected abstract void Log(ILogEntry entry);
}

public interface IChannelFilter
{
    bool CanLog<T>(ILogEntry entry) where T : IChannel;
}

public abstract class ChannelFilter : IChannelFilter
{
    public abstract bool CanLog<T>(ILogEntry entry) where T : IChannel;

    public class Empty : ChannelFilter
    {
        public override bool CanLog<T>(ILogEntry entry) => true;
    }
}