using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

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
            _data[name] = current.Add(new LogProperty(value, default(T)));
            return this;
        }

        public LogEntry Add(SoftString name, IEnumerable<LogProperty> properties)
        {
            var current = _data.TryGetValue(name, out var versions) ? versions : ImmutableList<LogProperty>.Empty;
            _data[name] = current.AddRange(properties);
            return this;
        }

        public LogEntry Clone() => new LogEntry(_data);

        public LogProperty GetPropertyOrDefault<T>(SoftString name) where T : struct, ILogPropertyAction
        {
            return
                TryGetProperty<T>(name, out var property)
                    ? property
                    : default; //throw DynamicException.Create("PropertyNotFound", $"There is no such property as '{name}'.");
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

        public override string ToString()
        {
            return @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}".Format(this);
        }

        public IEnumerator<KeyValuePair<SoftString, IImmutableList<LogProperty>>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class Names
        {
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string Logger = nameof(Logger);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string MessageBuilder = nameof(MessageBuilder);
            public static readonly string Exception = nameof(Exception);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);

            public static readonly string Object = nameof(Object);
            public static readonly string Snapshot = nameof(Snapshot);

            public static readonly string Scope = nameof(Scope);
            public static readonly string Elapsed = nameof(Stopwatch.Elapsed);
        }
    }

    public readonly struct LogProperty
    {
        public LogProperty(object value, ILogPropertyAction state)
        {
            Value = value;
            Action = state;
        }

        public object? Value { get; }

        public ILogPropertyAction Action { get; }

        public bool IsEmpty => Value is null && Action is null;
    }

    public static class LogPropertyExtensions
    {
        public static T ValueOrDefault<T>(this LogProperty property, T defaultValue = default)
        {
            return property.Value switch
            {
                T t => t,
                _ => defaultValue
                //_ => throw DynamicException.Create("LogProperty", $"Property value should be of type '{typeof(T).ToPrettyString()}' but is '{property.Value?.GetType().ToPrettyString()}'.")
            };
        }
    }

    public interface ILogPropertyAction { }

    namespace LogPropertyActions
    {
        /// <summary>
        /// Item is suitable for logging.
        /// </summary>
        public readonly struct Log : ILogPropertyAction { }

        /// <summary>
        /// Item needs to be exploded.
        /// </summary>
        public readonly struct Explode : ILogPropertyAction { }

        /// <summary>
        /// Item needs to be mapped or serialized.
        /// </summary>
        public readonly struct Serialize : ILogPropertyAction { }

        public readonly struct Copy : ILogPropertyAction { }

        public readonly struct Delete : ILogPropertyAction { }

        public readonly struct Build : ILogPropertyAction { }
    }

    public static class LogEntryExtensions
    {
        //public static IEnumerable<ItemKey<SoftString>> Keys(this LogEntry logEntry) => logEntry.Select(le => le.Key);

        public static IEnumerable<(SoftString Name, LogProperty Property)> Action<T>(this LogEntry entry) where T : struct, ILogPropertyAction
        {
            foreach (var (name, _) in entry.Select(x => (x.Key, x.Value)))
            {
                if (entry.TryGetProperty<T>(name, out var property))
                {
                    yield return (name, property);
                }
            }
        }
    }
}