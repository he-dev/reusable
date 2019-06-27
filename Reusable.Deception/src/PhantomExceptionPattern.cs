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
    public interface IPhantomExceptionPattern : IDisposable
    {
        bool Matches(string name);
    }

    [PublicAPI]
    [UsedImplicitly]
    public abstract class PhantomExceptionPattern<T> : IPhantomExceptionPattern, IDisposable
    {
        private IEnumerator<T> _values;

        protected PhantomExceptionPattern(IEnumerable<T> values)
        {
            _values = values.GetEnumerator();
            try
            {
                if (!_values.MoveNext())
                {
                    throw new ArgumentException
                    (
                        paramName: nameof(values),
                        message: $"Cannot initialize '{GetType().ToPrettyString()}' because there has to be at least one value."
                    );
                }
            }
            catch (Exception)
            {
                _values.Dispose();
                throw;
            }
            
            Reset();
        }

        [CanBeNull]
        public Func<string, bool> Predicate { get; set; }

        protected T Current => _values.Current;

        protected bool Eof => _values is null;

        public bool Matches(string name)
        {
            if (Eof) return false;
            if (Predicate?.Invoke(name) == false) return false;
            if (Matches() == false) return false;

            Reset();
            
            if (!_values.MoveNext())
            {
                _values.Dispose();
                _values = null;
            }
            
            return true;
        }

        protected abstract bool Matches();

        protected abstract void Reset();

        public abstract override string ToString();

        public void Dispose() => _values?.Dispose();
    }
}