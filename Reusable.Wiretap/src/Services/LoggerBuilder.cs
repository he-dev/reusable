using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Services;

public class LoggerBuilder : IEnumerable<IMiddleware>
{
    private List<IMiddleware> Middleware { get; } = new();

    public void Add(IMiddleware middleware) => this.Also(_ => Middleware.Add(middleware));

    public static LoggerBuilder CreateDefault() => new()
    {
        new AttachNodeId(),
        new AttachPrevId(),
        new AttachInstance(),
        new AttachTimestamp(),
        new AttachScope(),
        new AttachLevel(),
        new AttachElapsed(),
        new SerializeDetails(),
    };

    public IEnumerator<IMiddleware> GetEnumerator() => Middleware.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Middleware).GetEnumerator();

    public ILogger Build() => new Logger(this.Reverse().Aggregate((LogEntry _) => { }, (next, action) => entry => action.Invoke(entry, next)));
}