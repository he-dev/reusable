using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILinkedListNode<T> where T : class, ILinkedListNode<T>
    {
        T? Prev { get; set; }

        T? Next { get; set; }
    }

    public interface ILoggerNode : ILinkedListNode<ILoggerNode>, IDisposable
    {
        bool Enabled { get; set; }
        
        void Invoke(LogEntry request);
    }

    public static class LinkedListExtensions
    {
        public static T AddAfter<T>(this T a, T b) where T : class, ILinkedListNode<T>
        {
            var c = a.Next;

            b.Prev = a;
            b.Next = c;

            a.Next = b;
            if (c is {}) c.Prev = b;

            return b;
        }

        public static T AddBefore<T>(this T c, T b)where T : class, ILinkedListNode<T>
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

        public virtual ILoggerNode? Prev { get; set; }

        public virtual ILoggerNode? Next { get; set; }

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
}