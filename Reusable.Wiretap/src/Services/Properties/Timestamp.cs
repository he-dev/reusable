using System;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services.Properties;

public class Timestamp : PropertyService, IDisposable
{
    public Timestamp(IEnumerable<DateTime> timestamps) => Timestamps = timestamps.GetEnumerator();

    private IEnumerator<DateTime> Timestamps { get; }

    private int TimestampCount { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        if (Timestamps.MoveNext())
        {
            TimestampCount++;
            entry.Push(new LoggableProperty(nameof(Timestamp), Timestamps.Current));
        }
        else
        {
            throw new InvalidOperationException($"You provided only {TimestampCount} timestamps but more were required.");
        }
    }

    public void Dispose() => Timestamps.Dispose();
}