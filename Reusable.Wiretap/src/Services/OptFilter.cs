using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services;

public class OptFilter : ILogEntryFilter
{
    public bool CanLog(IChannel channel, LogEntry entry)
    {
        var oneTimeData = entry.Contexts.First().Get<IDictionary<string, object>>(nameof(UnitOfWork.Context.OneTimeData));
        if (oneTimeData.TryGetValue(nameof(Channel), out var channelType))
        {
            var channelMatches = (Type)channelType == channel.GetType();

            return oneTimeData[nameof(Opt)] switch
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