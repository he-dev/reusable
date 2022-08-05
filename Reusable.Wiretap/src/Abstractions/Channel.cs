using System;
using JetBrains.Annotations;
using Reusable.Marbles;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;


namespace Reusable.Wiretap.Abstractions;

public interface IChannel : ILoggerMiddleware { }

[PublicAPI]
public abstract class Channel : LoggerMiddleware, IChannel
{
    protected Channel(string? name) : base(name) { }

    public override void Invoke(ILogEntry entry)
    {
        if (CanLog(entry)) Log(entry);
    }

    protected abstract void Log(ILogEntry entry);

    private bool CanLog(ILogEntry entry)
    {
        var channelMatches = entry[LogProperty.Names.ChannelName()].Value is string name && SoftString.Comparer.Equals(name, Name);

        return entry[LogProperty.Names.ChannelMode()].Value switch
        {
            Mode.OptIn => channelMatches,
            Mode.OptOut => !channelMatches,
            _ => true
        };
    }

    public enum Mode
    {
        None,
        OptIn,
        OptOut
    }
}

public abstract class Channel<T> : Channel where T : IChannel
{
    protected Channel(string? name) : base(name) { }
}