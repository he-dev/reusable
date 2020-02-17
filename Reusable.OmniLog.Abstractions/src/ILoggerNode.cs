using System;
using System.Linq;
using Reusable.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerNode : INode<ILoggerNode>, IDisposable
    {
        bool Enabled { get; set; }

        void Invoke(ILogEntry request);
    }

    public abstract class LoggerNode : ILoggerNode
    {
        public virtual bool Enabled { get; set; } = true;

        public virtual ILoggerNode? Prev { get; set; }

        public virtual ILoggerNode? Next { get; set; }
        
        public abstract void Invoke(ILogEntry request);

        protected void InvokeNext(ILogEntry request)
        {
            if (this.EnumerateNext<ILoggerNode>(includeSelf: false).FirstOrDefault(n => n.Enabled) is {} next)
            {
                next.Invoke(request);
            }
        }

        public virtual void Dispose() => Next?.Dispose();
    }
}