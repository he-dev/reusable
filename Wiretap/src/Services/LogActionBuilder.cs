using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Modules;

namespace Reusable.Wiretap.Services;

public class LogActionBuilder : IEnumerable<IModule>
{
    private List<IModule> Modules { get; } = new();

    public void Add(IModule module) => this.Also(_ => Modules.Add(module));

    public static LogActionBuilder CreateDefault() => new()
    {
        new SetParent(),
        new SetUniqueId(),
        new SetTimestamp(),
        new SetElapsed(),
        new AttachDetails(),
        new InvokeWhen { Module = new AttachOwner(), Filter = new TraceFilter { Trace = Strings.Traces.Begin } },
        new SerializeDetails(),
        new SetExtra(),
    };

    public IEnumerator<IModule> GetEnumerator() => Modules.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Modules).GetEnumerator();

    public LogAction Build()
    {
        return this.Reverse().Aggregate(new LogAction(_ => { }), (next, middleware) => (context => middleware.Invoke(context, next)));
    }
}