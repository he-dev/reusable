using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
/// </summary>
public class ToggleScope : LoggerMiddleware
{
    public ToggleScope(params Func<ILoggerMiddleware>[] builders) => Builders = builders;

    private IEnumerable<Func<ILoggerMiddleware>> Builders { get; }

    public static ILoggerScope Current => AsyncScope<ILoggerScope>.Current.Value;

    public ILoggerScope Push() => AsyncScope<ILoggerScope>.Push(disposer => new LoggerScope(Builders.Invoke(), disposer)).Value;

    public override void Invoke(ILogEntry entry)
    {
        if (AsyncScope<ILoggerScope>.Exists)
        {
            entry.Push<IMetaProperty>(LogProperty.Names.LoggerScope(), Current);
            Current.Invoke(entry);
        }

        Next?.Invoke(entry);
    }
}