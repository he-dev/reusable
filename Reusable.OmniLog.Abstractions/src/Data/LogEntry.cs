using System;
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

        public LogEntry Add(LogProperty property)
        {
            var current = _data.TryGetValue(property.Name, out var versions) ? versions : ImmutableList<LogProperty>.Empty;
            _data[property.Name] = current.Add(property);
            return this;
        }


        public LogEntry Copy() => new LogEntry(_data);

        public bool TryGetProperty(SoftString name, out LogProperty property)
        {
            if (_data.TryGetValue(name, out var versions))
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

        public IEnumerator<KeyValuePair<SoftString, IImmutableList<LogProperty>>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class Names
        {
            public static readonly SoftString Timestamp = nameof(Timestamp)!;
            public static readonly SoftString Logger = nameof(Logger)!;
            public static readonly SoftString Level = nameof(Level)!;
            public static readonly SoftString Message = nameof(Message)!;
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
        public static bool TryGetProperty(this LogEntry entry, SoftString name, Action<LogPropertyMeta.LogPropertyMetaBuilder> meta, out LogProperty property)
        {
            if (entry.TryGetProperty(name, out property) && property.Meta.Contains(LogPropertyMeta.Builder.Pipe(meta)))
            {
                return true;
            }
            else
            {
                property = default;
                return false;
            }
        }

        public static LogProperty? GetProperty(this LogEntry entry, SoftString name, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return entry.TryGetProperty(name, buildMeta, out var property) ? property : default;
        }

        public static LogEntry Add(this LogEntry entry, SoftString name, object? value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return entry.Add(new LogProperty(name, value, LogPropertyMeta.Builder.Pipe(buildMeta)));
        }

        public static IEnumerable<LogProperty> Properties(this LogEntry entry, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            var meta = LogPropertyMeta.Builder.Pipe(buildMeta).Build();
            foreach (var property in entry.Select(x => x.Value))
            {
                if (property.Last() is var last && last.Meta.Contains(meta))
                {
                    yield return last;
                }
            }
        }
    }
}