using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reusable.OmniLog.v2
{
    public static class LoggerScope<T>
    {
        private static readonly AsyncLocal<Stack<T>> _current = new AsyncLocal<Stack<T>>
        {
            Value = new Stack<T>()
        };

        public static Stack<T> Current
        {
            get => _current.Value;
            //set => _current.Value = value;
        }

        public static bool IsEmpty => Current?.Any() != true;

        public static T Peek() => Current.Peek();

        public static T Push(T value)
        {
            Current.Push(value);
            return value;
        }

        public static T Pop()
        {
            if (!Current.Any())
            {
                throw new InvalidOperationException("This should not have occured. The scope seems to be disposed too many times");
            }

            return Current.Pop();
        }
    }
}