using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerNode : IDisposable
    {
        bool Enabled { get; set; }

        ILoggerNode? Prev { get; set; }

        ILoggerNode? Next { get; set; }

        void Invoke(LogEntry request);
    }

    public static class LoggerMiddlewareHelper
    {
        public static ILoggerNode AddAfter(this ILoggerNode a, ILoggerNode b)
        {
            var c = a.Next;

            b.Prev = a;
            b.Next = c;

            a.Next = b;
            if (c is {}) c.Prev = b;

            return b;
        }

        public static ILoggerNode AddBefore(this ILoggerNode c, ILoggerNode b)
        {
            var a = c.Prev;

            b.Prev = a;
            b.Next = c;

            if (a is {}) a.Next = b;
            c.Prev = b;

            return b;
        }

        public static IEnumerable<ILoggerNode> EnumerateNext(this ILoggerNode n, bool includeSelf = true) => n.Enumerate(x => x.Next, includeSelf);

        public static IEnumerable<ILoggerNode> EnumeratePrev(this ILoggerNode n, bool includeSelf = true) => n.Enumerate(x => x.Prev, includeSelf);

        private static IEnumerable<ILoggerNode> Enumerate(this ILoggerNode n, Func<ILoggerNode, ILoggerNode> direction, bool includeSelf = true)
        {
            n = includeSelf ? n : direction(n);
            while (n is {})
            {
                yield return n;
                n = direction(n);
            }
        }
    }

    public abstract class LoggerNode : ILoggerNode
    {
        public virtual bool Enabled { get; set; } = true;

        public ILoggerNode? Prev { get; set; }

        public ILoggerNode? Next { get; set; }

        // This being virtual makes testing easier.
        public virtual void Invoke(LogEntry request)
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
        protected abstract void invoke(LogEntry request);

        // ReSharper disable once InconsistentNaming
        protected void invokeNext(LogEntry request) => Next?.Invoke(request);

        public virtual void Dispose() => Next?.Dispose();
    }

    //    public abstract class LoggerNode : ILoggerMiddleware
    //    {
    //        public LoggerNode(bool enabled)
    //        {
    //            Enabled = enabled;
    //        }
    //        
    //        public virtual bool Enabled { get; set; } = true;
    //
    //        #region ILinkeListNode
    //
    //        [JsonIgnore]
    //        public LoggerNode Previous { get; private set; }
    //
    //        [JsonIgnore]
    //        public LoggerNode Next { get; private set; }
    //
    //        #endregion
    //
    //        // Inserts a new middleware after this one and returns the new one.
    //        public LoggerNode InsertNext(LoggerNode next)
    //        {
    //            (next.Previous, next.Next, Next) = (this, Next, next);
    //            return next;
    //        }
    //
    //        public LoggerNode Remove()
    //        {
    //            var result = default(LoggerNode);
    //
    //            if (!(Previous is null))
    //            {
    //                result = Previous;
    //                (Previous.Next, Previous) = (Next, null);
    //            }
    //
    //            if (!(Next is null))
    //            {
    //                result ??= Next;
    //                (Next.Previous, Next) = (Previous, null);
    //            }
    //
    //            return result;
    //        }
    //
    //        // It's easier to mock this with 'virtual'. 
    //        public virtual void Invoke(LogEntry request)
    //        {
    //            if (Enabled)
    //            {
    //                InvokeCore(request);
    //            }
    //            else
    //            {
    //                Next?.Invoke(request);
    //            }
    //        }
    //
    //        protected abstract void InvokeCore(LogEntry request);
    //
    //        // Removes itself from the middleware chain.
    //        public virtual void Dispose()
    //        {
    //            //Remove()
    //        }
    //    }

    public interface ILoggerQueue<in TItem>
    {
        void Enqueue(TItem item);
    }
}