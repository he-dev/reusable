using System;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Services.Properties;

public class Timestamp : PropertyService, IDisposable
{
    private int _timestampCount;

    private readonly IEnumerator<DateTime> _timestamps;
    
    public Timestamp(IEnumerable<DateTime> timestamps) : base(nameof(Timestamp))
    {
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