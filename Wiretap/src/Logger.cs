using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Reusable;

namespace Reusable.Wiretap;

public interface ILogger
{
    void Log(IEnumerable<KeyValuePair<string, object>> properties);
}

public class LoggerCollection(IEnumerable<ILogger> loggers) : ILogger
{
    public virtual void Log(IEnumerable<KeyValuePair<string, object>> properties)
    {
        var cache = new List<KeyValuePair<string, object>>();
        using var e = loggers.GetEnumerator();

        if (e.MoveNext())
        {
            e.Current.Log(properties.Select(x => x.Also(cache.Add)));

            while (e.MoveNext())
            {
                e.Current.Log(cache);
            }
        }
    }
}