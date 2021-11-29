using System;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Properties
{
    public class Timestamp : PropertyService, IDisposable
    {
        private int _timestampCount;

        private readonly IEnumerator<DateTime> _timestamps;

        // There is no pretty way to get the name without `1

        public Timestamp(IEnumerable<DateTime> timestamps) : base("Timestamp")
        {
            if (timestamps == null) throw new ArgumentNullException(nameof(timestamps));
            _timestamps = timestamps.GetEnumerator();
        }

        public override object? GetValue(ILogEntry logEntry)
        {
            if (_timestamps.MoveNext())
            {
                _timestampCount++;
                return _timestamps.Current;
            }

            throw new InvalidOperationException($"You provided only {_timestampCount} timestamps but more were required.");
        }

        public void Dispose() => _timestamps.Dispose();
    }
}