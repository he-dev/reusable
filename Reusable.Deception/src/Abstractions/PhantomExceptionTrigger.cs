using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Diagnostics.Abstractions
{
    public interface IPhantomExceptionTrigger
    {
        [CanBeNull]
        string Exception { get; }

        [CanBeNull]
        string Message { get; }

        bool CanThrow(IEnumerable<string> names);
    }

    [PublicAPI]
    [UsedImplicitly]
    public abstract class PhantomExceptionTrigger : IPhantomExceptionTrigger, IDisposable
    {
        private readonly IEnumerator<int> _sequence;

        private bool _hasElements;

        protected PhantomExceptionTrigger(IEnumerable<int> sequence, int count = 0)
        {
            _sequence = (count > 0 ? sequence.Take(count) : sequence).GetEnumerator();
            _hasElements = _sequence.MoveNext();
        }

        protected int Current => _sequence.Current;

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        #region IExceptionTrigger

        public string Exception { get; set; }

        public string Message { get; set; }

        #endregion

        [JsonRequired]
        public PhantomExceptionFilter Filter { get; set; }

        public bool CanThrow(IEnumerable<string> names)
        {
            if (Enabled && Filter.Matches(names) && _hasElements && CanThrow())
            {
                _hasElements = _sequence.MoveNext();
                return true;
            }

            return false;
        }

        protected abstract bool CanThrow();

        public abstract override string ToString();

        public void Dispose() => _sequence.Dispose();
    }
}