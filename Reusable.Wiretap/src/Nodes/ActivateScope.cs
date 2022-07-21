using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
/// </summary>
public class ActivateScope : LoggerNode, IEnumerable<Func<ILoggerNode>>
{
    private List<Func<ILoggerNode>> Nodes { get; } = new();

    public ILoggerScope Current => AsyncScope<ILoggerScope>.Current.Value;

    public ILoggerScope Push()
    {
        //var scope = new LoggerScope(this);

        //scope.First().Prev = this;
        //scope.Last().Next = Next;

        //return AsyncScope<ILoggerScope>.Push(new LoggerScope(this)).Value;
        return AsyncScope<ILoggerScope>.Push(disposer => new LoggerScope(this, disposer)).Value;
    }

    public override void Invoke(ILogEntry entry)
    {
        if (AsyncScope<ILoggerScope>.Exists)
        {
            entry.Push<IMetaProperty>(LogProperty.Names.LoggerScope(), Current);
            Current.Invoke(entry);
        }

        Next?.Invoke(entry);
    }

    public void Add(Func<ILoggerNode> node) => Nodes.Add(node);

    public IEnumerator<Func<ILoggerNode>> GetEnumerator() => Nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}