using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reusable.OmniLog.v2
{
    public class LoggerScope<T>
    {
        private static readonly AsyncLocal<LoggerScope<T>> State = new AsyncLocal<LoggerScope<T>>();

        private LoggerScope(T value)
        {
            Value = value;
        }

        public T Value { get; }


        public LoggerScope<T> Parent { get; private set; }

        public static LoggerScope<T> Current
        {
            get => State.Value;
            private set => State.Value = value;
        }

        public static LoggerScope<T> Push(T value)
        {
            return Current = new LoggerScope<T>(value) { Parent = Current };
        }

        public void Dispose()
        {
            Current = Current?.Parent;
        }
    }

    // -------------- these didn't work ---------------


//    public static class LoggerScope<T>
//    {
//        private static readonly AsyncLocal<Stack<T>> _current = new AsyncLocal<Stack<T>> { Value = new Stack<T>() };
//
//
//        private static Stack<T> Current
//        {
//            get => _current.Value;
//            //set => _current.Value = value;
//        }
//
//        public static bool IsEmpty => Current?.Any() != true;
//
//        public static T Peek() => Current.Peek();
//
//        public static T Push(T value)
//        {
//            // Due to some strange circumstances, Value isn't always initialized.
//            if (_current.Value == null)
//            {
//                _current.Value = new Stack<T>();
//            }
//
//            _current.Value.Push(value);
//            return value;
//        }
//
//        public static T Pop()
//        {
//            if (!Current.Any())
//            {
//                throw new InvalidOperationException("This should not have occured. The scope seems to be disposed too many times");
//            }
//
//            return Current.Pop();
//        }
//    }
//
//    public static class LoggerScopeHelper2
//    {
//        public static Stack<T> Current<T>(this AsyncLocal<Stack<T>> asyncLocal) => asyncLocal.Value;
//
//        public static bool IsEmpty<T>(this AsyncLocal<Stack<T>> asyncLocal) => asyncLocal.Current()?.Any() != true;
//
//        public static T Peek<T>(this AsyncLocal<Stack<T>> asyncLocal) => asyncLocal.Current().Peek();
//
//        public static T Push<T>(this AsyncLocal<Stack<T>> asyncLocal, T value)
//        {
//            asyncLocal.Current().Push(value);
//            return value;
//        }
//
//        public static T Pop<T>(this AsyncLocal<Stack<T>> asyncLocal)
//        {
//            if (asyncLocal.IsEmpty())
//            {
//                throw new InvalidOperationException("This should not have occured. The scope seems to be disposed too many times");
//            }
//
//            return asyncLocal.Current().Pop();
//        }
//    }
}