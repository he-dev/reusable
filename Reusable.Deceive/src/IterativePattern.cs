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
    [PublicAPI]
    [UsedImplicitly]
    public abstract class IterativePattern<T> : IPhantomExceptionPattern
    {
        private IEnumerator<T> _values;

        protected IterativePattern(IEnumerable<T> values)
        {
            _values = values.GetEnumerator();

            if (!_values.MoveNext())
            {
                throw new ArgumentException
                (
                    paramName: nameof(values),
                    message: $"Cannot initialize '{GetType().ToPrettyString()}' because there has to be at least one value."
                );
            }
        }

        protected T Current => _values.Current;

        protected bool Eof => _values is null;

        public bool Matches(string name, params string[] tags)
        {
            if (Eof || !Matches()) return false;

            if (!_values.MoveNext())
            {
                _values.Dispose();
                _values = null;
            }

            return true;
        }

        protected abstract bool Matches();
        
        public override string ToString()=> $"{GetType().ToPrettyString()}: '{Current}'.";

        public void Dispose() => _values?.Dispose();
    }
}