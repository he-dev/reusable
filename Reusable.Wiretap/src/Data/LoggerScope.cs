using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public class LoggerScope : ILoggerScope
{

    public LoggerScope(IEnumerable<Func<ILoggerNode>> nodes, IDisposable disposer)
    {
        Disposer = disposer;
        //Nodes = nodes.Select(f => f()).ToList();
        First = nodes.Select(f => f()).Join().First();
    }

    private IDisposable Disposer { get; }

    //private IEnumerable<ILoggerNode> Nodes { get; }

    private ILoggerNode First { get; }

    public IEnumerable<ILoggerScope> Parents => AsyncScope<ILoggerScope>.Current.Enumerate().Select(s => s.Value);

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public void Invoke(ILogEntry entry) => First.Invoke(entry);

    public IEnumerator<ILoggerNode> GetEnumerator() => First.EnumerateNext().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public virtual void Dispose()
    {
        foreach (var node in this) node.Dispose();
        foreach (var item in Items.Values.OfType<IDisposable>()) item.Dispose();

        Items.Clear();

        Disposer.Dispose();

        //AsyncScope<ILoggerScope>.Current.Dispose();
    }

    public class Empty : LoggerScope
    {
        private Empty() : base(Enumerable.Empty<Func<ILoggerNode>>(), Disposable.Empty) { }

        public static readonly ILoggerScope Instance = new Empty();

        public override void Dispose() { }
    }
}