using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    [PublicAPI]
    public class LogEntry : ILogEntry
    {
        private readonly IDictionary<SoftString, IImmutableList<LogProperty>> _properties;

        [DebuggerStepThrough]
        public LogEntry()
        {
            _properties = new Dictionary<SoftString, IImmutableList<LogProperty>>();
        }

        private LogEntry(LogEntry other)
        {
            _properties = new Dictionary<SoftString, IImmutableList<LogProperty>>(other._properties);
        }

        public static LogEntry Empty() => new LogEntry();

        public LogProperty? this[SoftString key] => TryGetProperty(key, out var property) ? property : default;

        public void Add(LogProperty property)
        {
            var current = _properties.TryGetValue(property.Name, out var versions) ? versions : ImmutableList<LogProperty>.Empty;
            _properties[property.Name] = current.Add(property);
        }

        public ILogEntry Copy() => new LogEntry(this);

        public bool TryGetProperty(SoftString name, out LogProperty property)
        {
            if (_properties.TryGetValue(name, out var versions))
            {
                property = versions.Last();
                return true;
            }
            else
            {
                property = default;
                return false;
            }
        }

        public override string ToString()
        {
            return @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}".Format(this);
        }

        public IEnumerator<LogProperty> GetEnumerator() => _properties.Values.Select(h => h.Last()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}