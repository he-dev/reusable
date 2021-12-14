using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
/// </summary>
public class ToggleScope : LoggerNode
{
    public override bool Enabled => AsyncScope<ILoggerScope>.Any;

    public List<Func<ILoggerNode>> ScopeFactories { get; set; } = new();

    public ILoggerScope Current => AsyncScope<ILoggerScope>.Current?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Current)} when {nameof(ToggleScope)} is disabled. Create one with Logger.BeginScope() first.");

    public ILoggerScope Push(ILogger logger, string name)
    {
        var scope = new LoggerScope(name, ScopeFactories.Select(f => f()));

        scope.First.Prev = this;
        scope.First.Last().Next = Next;

        return AsyncScope<ILoggerScope>.Push(scope).Value;
    }

    public override void Invoke(ILogEntry entry)
    {
        // Does not call InvokeNext because it routes the request over the scope which is connected to the next node.
        Current.First.Invoke(entry.Push(new MetaProperty.Scope(Current)));
    }
}