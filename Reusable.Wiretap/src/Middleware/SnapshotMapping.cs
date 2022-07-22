using System;
using System.Linq.Expressions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public class SnapshotMapping<T> : SnapshotMapping
{
    public SnapshotMapping(Func<object, object> map) => Map = map;

    private Func<object, object> Map { get; }

    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryPeek(LogProperty.Names.Snapshot(), out var property) && property is T snapshot)
        {
            var result = Map(snapshot);
            entry.Push<ITransientProperty>(LogProperty.Names.Snapshot(), result);
        }

        Next?.Invoke(entry);
    }
}

public abstract class SnapshotMapping : LoggerMiddleware
{
    public static SnapshotMapping For<T>(Func<T, object> map)
    {
        // We can store only Func<object, object> but the call should be able
        // to use T so we need to cast the parameter from 'object' to T.

        // Compile: map((T)obj)
        var parameter = Expression.Parameter(typeof(object), "obj");
        var mapFunc =
            Expression.Lambda<Func<object, object>>(
                    Expression.Call(
                        Expression.Constant(map.Target),
                        map.Method,
                        Expression.Convert(parameter, typeof(T))),
                    parameter)
                .Compile();

        return new SnapshotMapping<T>(mapFunc);
    }
}