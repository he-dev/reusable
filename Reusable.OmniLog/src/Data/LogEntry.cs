using System;
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
        private readonly IDictionary<string, IImmutableStack<LogProperty>> _properties;
        private readonly IDictionary<Type, IImmutableStack<string>> _index = new Dictionary<Type, IImmutableStack<string>>();

        [DebuggerStepThrough]
        public LogEntry()
        {
            _properties = new Dictionary<string, IImmutableStack<LogProperty>>(SoftString.Comparer);
        }

        private LogEntry(LogEntry other)
        {
            _properties = new Dictionary<string, IImmutableStack<LogProperty>>(other._properties, SoftString.Comparer);
        }

        public static LogEntry Empty() => new LogEntry();
        
        public LogProperty? this[string key] => TryGetProperty(key, out var property) ? property : default;

        public void Add(LogProperty property)
        {
            var current = _properties.TryGetValue(property.Name, out var versions) ? versions : ImmutableStack<LogProperty>.Empty;
            _properties[property.Name] = current.Push(property);
            
        }

        public ILogEntry Copy() => new LogEntry(this);

        public bool TryGetProperty(string name, out LogProperty property)
        {
            if (_properties.TryGetValue(name, out var versions))
            {
                property = versions.Peek();
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

        public IEnumerator<LogProperty> GetEnumerator() => _properties.Values.Select(h => h.Peek()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        
        
    }
}