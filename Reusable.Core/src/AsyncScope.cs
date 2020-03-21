using System;
using System.Threading;
using Reusable.Extensions;

namespace Reusable
{
    public class AsyncScope<T> : IDisposable
    {
        private static readonly AsyncLocal<AsyncScope<T>> State = new AsyncLocal<AsyncScope<T>>();

        private AsyncScope(T value) => Value = value;

        public T Value { get; }

        public AsyncScope<T>? Parent { get; private set; }

        public static AsyncScope<T>? Current
        {
            get => State.Value;
            private set => State.Value = value!;
        }

        /// <summary>
        /// Gets a value indicating whether there are any states on the stack.
        /// </summary>
        public static bool Any => Current is {};

        public static AsyncScope<T> Push(T value)
        {
            return Current = new AsyncScope<T>(value) { Parent = Current };
        }

        public void Dispose() => Current = Current?.Parent;

        public static implicit operator T(AsyncScope<T> scope) => scope.Value;
    }
}