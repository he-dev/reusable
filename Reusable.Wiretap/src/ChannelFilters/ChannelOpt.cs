using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.ChannelFilters;

/// <summary>
/// Allows to opt for a channel.
/// </summary>
public class ChannelOpt : IChannelFilter
{
    public bool CanLog<T>(ILogEntry entry) where T : IChannel
    {
        var name = entry[LogProperty.Names.ChannelName()].Value as string;
        var channelMatches = entry.TryPeek($"{typeof(T)}/{name}/{nameof(ChannelOpt)}", out var property);

        return property.Value switch
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