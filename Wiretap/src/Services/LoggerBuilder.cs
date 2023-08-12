using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Modules;

namespace Reusable.Wiretap.Services;

public class LoggerBuilder : IEnumerable<IModule>
{
    private List<IModule> Modules { get; } = new();

    public void Add(IModule module) => this.Also(_ => Modules.Add(module));

    public static LoggerBuilder CreateDefault() => new()
    {
        new SetUniqueId(),
        new SetParent(),
        new SetTimestamp(),
        //new SetLevel(),
        new SetElapsed(),
        new AttachDetails(),
        new AttachProperties(),
        new AttachCallerInfo(),
        new SerializeDetails(),
    };

    public IEnumerator<IModule> GetEnumerator() => Modules.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Modules).GetEnumerator();

    public ILogger Build()
    {
        var log = this.Reverse().Aggregate(new LogFunc((_, _) => { }), (next, middleware) => (flow, entry) => middleware.Invoke(flow, entry, next));
        return new Logger { Log = log };
    }
}