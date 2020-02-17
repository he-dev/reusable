using System;
using System.Collections.Generic;
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

        public virtual ILoggerNode Prev { get; set; }

        public virtual ILoggerNode Next { get; set; }

        // This being virtual makes testing easier.
        public virtual void Invoke(ILogEntry request)
        {
            if (Enabled)
            {
                invoke(request);
            }
            else
            {
                invokeNext(request);
            }
        }

        // ReSharper disable once InconsistentNaming
        protected abstract void invoke(ILogEntry request);

        // ReSharper disable once InconsistentNaming
        protected void invokeNext(ILogEntry request)
        {
            if (Next is {} node)
            {
                node.Invoke(request);
            }
        }

        public virtual void Dispose() => Next?.Dispose();
    }
}