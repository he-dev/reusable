using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap;

public class LoggerContext : IDisposable, IEnumerable<LoggerContext>
{
    private static readonly IEnumerable<string> Finalizers = new[]
    {
        nameof(TaskStatus.Completed),
        nameof(TaskStatus.Canceled),
        nameof(TaskStatus.Faulted)
    };

    public LoggerContext(LogDelegate log, string name)
    {
        Log = log;
        Properties.Scoped.Add(AttachScope.Key, name);
        Scope = AsyncScope.Push(this);
    }

    private LogDelegate Log { get; }

    public Data Properties { get; } = new();

    private IDisposable Scope { get; }

    public void Status(string name, object? details, object? attachment)
    {
        Properties.Scoped.Set(nameof(Status), name);

        Log(
            new LogEntry(this, name)
                .SetItem(nameof(details), details.EnumerateProperties().ToImmutableDictionary<string, object?>(SoftString.Comparer))
                .SetItem(nameof(attachment), attachment));

        Properties.Transient.Dispose();
        Properties.Transient = new Container();
    }

    public void Dispose()
    {
        // Finalize when the user hasn't already done that.
        if (Properties.Scoped.TryGetItem(nameof(Status), out string status) && !status.In(Finalizers))
        {
            if (Properties.Scoped.TryGetItem<Exception>(nameof(Exception), out var exception))
            {
                this.Faulted(attachment: exception);
            }
            else
            {
                this.Ended();
            }
        }

        Scope.Dispose();
    }

    public IEnumerator<LoggerContext> GetEnumerator() => AsyncScope<LoggerContext>.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class Data
    {
        public Container Scoped { get; } = new();

        public Container Transient { get; internal set; } = new();
    }

    public class Container : IDisposable
    {
        private IDictionary<string, object> Data { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public bool ContainsKey(string key) => Data.ContainsKey(key);

        public TItem GetOrAdd<TItem>(string key, Func<TItem> factory)
        {
            if (!Data.TryGetValue(key, out var result))
            {
                result = factory();
                Data[key] = result;
            }

            return (TItem)result;
        }

        public TItem Get<TItem>(string key)
        {
            if (Data.TryGetValue(key, out var value))
            {
                if (value is TItem item)
                {
                    return item;
                }

                throw new InvalidCastException($"Cannot cast '{key}' to '{typeof(TItem)}'.");
            }

            throw new KeyNotFoundException($"Key '{key}' not found..");
        }

        public TItem Set<TItem>(string key, TItem value)
        {
            try
            {
                return value;
            }
            finally
            {
                Data[key] = value;
            }
        }

        public TItem Add<TItem>(string key, TItem value) => GetOrAdd(key, () => value);

        public bool TryGetItem<TItem>(string key, [MaybeNullWhen(false)] out TItem item)
        {
            if (Data.TryGetValue(key, out var value))
            {
                item = (TItem)value;
                return true;
            }

            item = default;
            return false;
        }

        public void Dispose()
        {
            foreach (var disposable in Data.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            Data.Clear();
        }
    }
}