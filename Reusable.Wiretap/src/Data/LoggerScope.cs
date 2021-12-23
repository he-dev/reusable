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
    public LoggerScope(string name, IEnumerable<ILoggerNode> nodes)
    {
        Name = name;
        Nodes = nodes.ToList();
        First = Nodes.Join().First();
    }

    private IEnumerable<ILoggerNode> Nodes { get; }
    
    public string Name { get; }
    
    public ILoggerNode First { get; }

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);
    
    public IEnumerator<ILoggerScope> GetEnumerator() => AsyncScope<ILoggerScope>.Current.Enumerate().Select(s => s.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        foreach (var node in Nodes) node.Dispose();
        foreach (var item in Items) (item.Value as IDisposable)?.Dispose();

        AsyncScope<ILoggerScope>.Current?.Dispose();
    }
}