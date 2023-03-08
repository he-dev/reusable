using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Services;

public class LoggerBuilder : IEnumerable<IMiddleware>
{
    private List<IMiddleware> Middleware { get; } = new();

    public void Add(IMiddleware middleware) => this.Also(_ => Middleware.Add(middleware));

    public static LoggerBuilder CreateDefault() => new()
    {
        new AttachNode(),
        new AttachParent(),
        new AttachTimestamp(),
        new AttachScope(),
        new AttachLevel(),
        new AttachElapsed(),
        new AttachCorrelationId(),
        new SerializeDetails(),
    };

    public IEnumerator<IMiddleware> GetEnumerator() => Middleware.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Middleware).GetEnumerator();

    public ILogger Build()
    {
        var log = this.Reverse().Aggregate(new LogDelegate(_ => { }), (next, middleware) => entry => middleware.Invoke(entry, next));
        return new Logger(log);
    }
}