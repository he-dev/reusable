using System;
using System.Linq;
using Reusable.Collections.Generic;

namespace Reusable.Wiretap.Abstractions
{
    public interface ILoggerNode : INode<ILoggerNode>, IDisposable
    {
        bool Enabled { get; set; }

        void Invoke(ILogEntry entry);
    }

    public abstract class LoggerNode : ILoggerNode
    {
        public virtual bool Enabled { get; set; } = true;

        public virtual ILoggerNode? Prev { get; set; }

        public virtual ILoggerNode? Next { get; set; }
        
        public abstract void Invoke(ILogEntry entry);

        protected void InvokeNext(ILogEntry request)
        {
            if (this.EnumerateNext<ILoggerNode>(includeSelf: false).FirstOrDefault(n => n.Enabled) is {} next)
            {
                next.Invoke(request);
            }
        }

        public virtual void Dispose()
        {
            //((ILoggerNode)this).Remove();
        }
    }
}