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
        Properties.Scoped.Set(AttachScope.Key, name);
        Scope = AsyncScope.Push(this);
    }

    private LogDelegate Log { get; }

    public PropertyGroup Properties { get; } = new();

    private IDisposable Scope { get; }

    public void Status(string name, object? details, object? attachment)
    {
        Properties.Scoped.Set(nameof(Status), name);

        Log(
            LogEntry
                .Create(this, name)
                .SetItem(nameof(details), details.EnumerateProperties().ToImmutableDictionary(SoftString.Comparer))
                .SetItem(nameof(attachment), attachment));

        Properties.Transient = new Container();
    }

    public void Dispose()
    {
        // Finalize when the user hasn't already done that.
        if (Properties.Scoped.TryGetItem(nameof(Status), out string? status) && !status.In(Finalizers))
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

        Properties.Dispose();
        Scope.Dispose();
    }

    public IEnumerator<LoggerContext> GetEnumerator() => AsyncScope<LoggerContext>.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class PropertyGroup : IDisposable
    {
        private Container _transient = new();
        
        public Container Scoped { get; } = new();

        public Container Transient
        {
            get => _transient;
            internal set
            {
                _transient.Dispose();
                _transient = value;
            }
        }

        public void Dispose()
        {
            Scoped.Dispose();
            Transient.Dispose();
        }
    }

    public class Container : IDisposable
    {
        private IDictionary<string, object> Data { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public bool ContainsKey(string key) => Data.ContainsKey(key);

        public TItem? GetOrAdd<TItem>(string key, Func<TItem> factory)
        {
            if (Data.TryGetValue(key, out var result))
            {
                return (TItem?)result;
            }

            if (factory() is { } value)
            {
                return Set(key, value).Get<TItem>(key);
            }

            throw new ArgumentException("Value must not be null.", paramName: nameof(factory));
        }

        public TItem Get<TItem>(string key)
        {
            if (Data.TryGetValue(key, out var value))
            {
                if (value is TItem item)
                {
                    return item;
                }

                throw new InvalidCastException($"Cannot cast '{key}' from '{value.GetType()}' to '{typeof(TItem)}'.");
            }

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        public Container Set<TItem>(string key, TItem? value)
        {
            if (value is { })
            {
                Data[key] = value;
            }

            return this;
        }

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