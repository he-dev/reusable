using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.Deception
{
    public delegate bool TryMoveNext<T>(out T next);

    public interface IPhantomExceptionTrigger : IDisposable
    {
        [CanBeNull]
        string Exception { get; }

        [CanBeNull]
        string Message { get; }

        bool CanThrow(string name);
    }

    [PublicAPI]
    [UsedImplicitly]
    public abstract class PhantomExceptionTrigger<T> : IPhantomExceptionTrigger, IDisposable
    {
        private readonly IEnumerator<T> _values;

        protected PhantomExceptionTrigger(IEnumerable<T> values)
        {
            _values = values.GetEnumerator();
            if (!_values.MoveNext())
            {
                throw new ArgumentException("Counter cannot start.");
            }
        }

        protected T Current => _values.Current;

        protected bool Eof { get; private set; }

        #region IExceptionTrigger

        public string Exception { get; set; }

        public string Message { get; set; }

        #endregion

        [CanBeNull]
        public Func<string, bool> Filter { get; set; }

        public bool CanThrow(string name)
        {
            if (Eof) return false;
            if (Filter?.Invoke(name) == false) return false;
            if (CanThrow() == false) return false;

            Reset();
            Eof = !_values.MoveNext();
            return true;
        }

        protected abstract bool CanThrow();

        protected abstract void Reset();

        protected bool MoveNext() => _values.MoveNext().Do(x => Eof = !x);

        public abstract override string ToString();

        public void Dispose() => _values.Dispose();
    }
}