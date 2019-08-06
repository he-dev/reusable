using System;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.v2;

namespace Reusable.OmniLog.Abstractions
{
    public abstract class LoggerMiddleware : ILinkedListNode<LoggerMiddleware>, IDisposable
    {
        public LoggerMiddleware(bool isActive)
        {
            IsActive = isActive;
        }
        
        public virtual bool IsActive { get; set; } = true;

        #region ILinkeListNode

        [JsonIgnore]
        public LoggerMiddleware Previous { get; private set; }

        [JsonIgnore]
        public LoggerMiddleware Next { get; private set; }

        #endregion

        // Inserts a new middleware after this one and returns the new one.
        public LoggerMiddleware InsertNext(LoggerMiddleware next)
        {
            (next.Previous, next.Next, Next) = (this, Next, next);
            return next;
        }

        public LoggerMiddleware Remove()
        {
            var result = default(LoggerMiddleware);

            if (!(Previous is null))
            {
                result = Previous;
                (Previous.Next, Previous) = (Next, null);
            }

            if (!(Next is null))
            {
                result = result ?? Next;
                (Next.Previous, Next) = (Previous, null);
            }

            return result;
        }

        public void Invoke(LogEntry request)
        {
            if (IsActive)
            {
                InvokeCore(request);
            }
            else
            {
                Next?.Invoke(request);
            }
        }

        protected abstract void InvokeCore(LogEntry request);

        // Removes itself from the middleware chain.
        public virtual void Dispose()
        {
            Remove();
        }
    }

    public interface ILoggerQueue<in TItem>
    {
        void Enqueue(TItem item);
    }
}