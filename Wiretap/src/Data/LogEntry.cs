using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Reusable.Wiretap.Data;

public record LogEntry(IImmutableDictionary<string, object?> Data) : IEnumerable<KeyValuePair<string, object?>>
{
    public object? this[string key] => TryGetValue(key, out var value) ? value : default;

    public LogEntry SetItem(string key, object? value) => new(Data.SetItem(key, value));

    public LogEntry RemoveItem(string key) => new(Data.Remove(key));

    public bool TryGetValue(string key, out object? value) => Data.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();

    public static LogEntry Empty() => new(ImmutableDictionary<string, object?>.Empty);
    
    public static class PropertyNames
    {
        public const string Message = nameof(Message);
        public const string Trace = nameof(Trace);
        public const string Details = nameof(Details);
        public const string Attachment = nameof(Attachment);
    }
}