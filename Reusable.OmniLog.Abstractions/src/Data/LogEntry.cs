using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.OmniLog.Abstractions.Data
{
    [PublicAPI]
    public class LogEntry : IEnumerable<KeyValuePair<SoftString, IImmutableList<LogProperty>>>
    {
        private readonly IDictionary<SoftString, IImmutableList<LogProperty>> _data;

        [DebuggerStepThrough]
        public LogEntry()
        {
            _data = new Dictionary<SoftString, IImmutableList<LogProperty>>();
        }

        private LogEntry(IDictionary<SoftString, IImmutableList<LogProperty>> data)
        {
            _data = new Dictionary<SoftString, IImmutableList<LogProperty>>(data);
        }

        public static LogEntry Empty() => new LogEntry();

        public IImmutableList<LogProperty> this[SoftString key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public LogEntry Add<T>(SoftString name, object? value) where T : struct, ILogPropertyAction
        {
            var current = _data.TryGetValue(name, out var property) ? property : ImmutableList<LogProperty>.Empty;
            _data[name] = current.Add(new LogProperty(name, value, default(T)));
            return this;
        }
        
        public LogEntry Add<T>(string name, object? value) where T : struct, ILogPropertyAction
        {
            return Add<T>(name.ToSoftString()!, value);
        }

        public LogEntry Add(SoftString name, IEnumerable<LogProperty> properties)
        {
            var current = _data.TryGetValue(name, out var versions) ? versions : ImmutableList<LogProperty>.Empty;
            _data[name] = current.AddRange(properties);
            return this;
        }

        public LogEntry Copy() => new LogEntry(_data);

        public LogProperty GetPropertyOrDefault<T>(SoftString name) where T : struct, ILogPropertyAction
        {
            return
                TryGetProperty<T>(name, out var property)
                    ? property
                    : default;
        }

        public bool TryGetProperty<T>(SoftString name, out LogProperty property) where T : struct, ILogPropertyAction
        {
            if (_data.TryGetValue(name, out var propertyVersions))
            {
                property = propertyVersions.LastOrDefault() is {} version && version.Action.Equals(default(T)) ? version : default;
                return !property.IsEmpty;
            }

            property = default;
            return false;
        }
        
        public LogProperty? LastOf(SoftString name)
        {
            return _data.TryGetValue(name, out var propertyVersions) ? propertyVersions.Last() : default;
        }

        public override string ToString()
        {
            return @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}".Format(this);
        }

        public IEnumerator<KeyValuePair<SoftString, IImmutableList<LogProperty>>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class Names
        {
            public static readonly SoftString Timestamp = nameof(Timestamp)!;
            public static readonly SoftString Logger = nameof(Logger)!;
            public static readonly SoftString Level = nameof(Level)!;
            public static readonly SoftString Message = nameof(Message)!;
            public static readonly SoftString MessageBuilder = nameof(MessageBuilder)!;
            public static readonly SoftString Exception = nameof(Exception)!;
            public static readonly SoftString CallerMemberName = nameof(CallerMemberName)!;
            public static readonly SoftString CallerLineNumber = nameof(CallerLineNumber)!;
            public static readonly SoftString CallerFilePath = nameof(CallerFilePath)!;

            public static readonly SoftString SnapshotName = nameof(SnapshotName)!;
            public static readonly SoftString Snapshot = nameof(Snapshot)!;

            public static readonly SoftString Scope = nameof(Scope)!;
            public static readonly SoftString Elapsed = nameof(Stopwatch.Elapsed)!;
        }
    }

    public static class LogEntryExtensions
    {
        /// <summary>
        /// Enumerates log-properties where the specified action is the last one.
        /// </summary>
        public static IEnumerable<LogProperty> Action<T>(this LogEntry entry) where T : struct, ILogPropertyAction
        {
            foreach (var name in entry.Select(x => x.Key))
            {
                if (entry.TryGetProperty<T>(name, out var property))
                {
                    yield return property;
                }
            }
        }
        
        public static IEnumerable<LogProperty> PropertiesHandledBy<T>(this LogEntry entry) where T : ILoggerNode
        {
            foreach (var name in entry.Select(x => x.Key))
            {
                if (entry.LastOf(name) is {} last && last.Action is T)
                {
                    yield return last;
                }
            }
        }
    }
}