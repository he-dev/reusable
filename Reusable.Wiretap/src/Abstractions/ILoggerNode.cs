using System;
using Reusable.Essentials;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerNode : IListNode<ILoggerNode>, IDisposable
{
    void Invoke(ILogEntry entry);
}

public abstract class LoggerNode : ILoggerNode
{
    public virtual ILoggerNode? Prev { get; set; }

    public virtual ILoggerNode? Next { get; set; }

    public abstract void Invoke(ILogEntry entry);

    public virtual void Dispose() { }

    public class Empty : LoggerNode
    {
        private Empty() { }

        public override void Invoke(ILogEntry entry) => Next?.Invoke(entry);

        public static readonly ILoggerNode Instance = new Empty();
    }
}