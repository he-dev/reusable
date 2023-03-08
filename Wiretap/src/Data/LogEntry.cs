using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.Wiretap.Data;

public class LogEntry : IEnumerable<KeyValuePair<string, object?>>
{
    private LogEntry(IImmutableDictionary<string, object?> data, LoggerContext context)
    {
        Data = data;
        Context = context;
    }

    public LogEntry(LoggerContext context, string status)
        : this(
            ImmutableDictionary
                .Create<string, object?>(SoftString.Comparer)
                .SetItem(nameof(status), status),
            context
        ) { }

    private IImmutableDictionary<string, object?> Data { get; }

    public LoggerContext Context { get; }

    public string Status => (string)Data[nameof(Status)]!;

    public LogEntry SetItem(string key, object? value) => new(Data.SetItem(key, value), Context);
    
    public LogEntry RemoveItem(string key) => new(Data.Remove(key), Context);

    public bool TryGetValue(string key, out object? value) => Data.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
}

