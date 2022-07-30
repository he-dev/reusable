using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerMiddleware : IListNode<ILoggerMiddleware>, IEnumerable<ILoggerMiddleware>
{
    string Name { get; }
    
    void Invoke(ILogEntry entry);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class LoggerMiddleware : ListNode<ILoggerMiddleware>, ILoggerMiddleware
{
    protected LoggerMiddleware(string? name = default)
    {
        Name = name is { } ? $"{GetType().ToPrettyString()}[{name}]" : GetType().ToPrettyString();
    }

    public virtual string Name { get; }
    
    public abstract void Invoke(ILogEntry entry);

    public class Empty : LoggerMiddleware
    {
        private Empty() { }

        public override void Invoke(ILogEntry entry) => Next?.Invoke(entry);

        public static readonly ILoggerMiddleware Instance = new Empty();
    }

    public IEnumerator<ILoggerMiddleware> GetEnumerator() => ((ILoggerMiddleware)this).EnumerateNext().GetEnumerator();
}