using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    [PublicAPI]
    public class LogEntry : ILogEntry
    {
        private readonly IDictionary<string, IImmutableStack<LogProperty>> _properties;

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

        public LogProperty this[string name] => TryGetProperty(name, out var property) ? property : throw DynamicException.Create("PropertyNotFound", $"There is no property with the name '{name}'.");

        public void Push(LogProperty property)
        {
            var current = _properties.TryGetValue(property.Name, out var versions) ? versions : ImmutableStack<LogProperty>.Empty;
            _properties[property.Name] = current.Push(property);
        }

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
            return @"[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}".Format(this);
        }

        public IEnumerator<LogProperty> GetEnumerator() => _properties.Values.Select(versions => versions.Peek()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}