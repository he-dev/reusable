using System;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services;

public class OptFilter : ILogEntryFilter
{
    public bool CanLog(IChannel channel, LogEntry entry)
    {
        var transient = entry.Context.First().Properties.Transient;
        if (transient.TryGetItem<Type>(nameof(Channel), out var channelType))
        {
            var channelMatches = channelType == channel.GetType();
            return transient.Get<Opt>(nameof(Opt)) switch
            {
                Opt.In => channelMatches,
                Opt.Out => !channelMatches,
                _ => true
            };
        }

        return true;
    }
}

public enum Opt
{
    None,
    In,
    Out
}