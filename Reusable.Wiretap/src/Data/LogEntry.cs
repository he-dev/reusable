using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Marbles;

namespace Reusable.Wiretap.Data;

public class LogEntry : IEnumerable<KeyValuePair<string, object?>>
{
    private LogEntry(IImmutableDictionary<string, object?> data, IEnumerable<IMemoryCache> contexts)
    {
        Data = data;
        Contexts = contexts;
    }

    public LogEntry(string status, IEnumerable<IMemoryCache> contexts)
        : this(
            ImmutableDictionary
                .Create<string, object?>(SoftString.Comparer)
                .SetItem(nameof(status), status), contexts) 
    { }

    private IImmutableDictionary<string, object?> Data { get; }

    public IEnumerable<IMemoryCache> Contexts { get; }

    public string Status => (string)Data[nameof(Status)]!;

    public LogEntry SetItem(string key, object? value) => new(Data.SetItem(key, value), Contexts);

    public bool TryGetValue(string key, out object? value) => Data.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
}