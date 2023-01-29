using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap;

public class UnitOfWork
{
    public UnitOfWork(ILogger logger, Func<IMemoryCache> createCache)
    {
        Logger = logger;
        CreateCache = createCache;
    }

    private ILogger Logger { get; }

    private Func<IMemoryCache> CreateCache { get; }

    [MustUseReturnValue]
    public Context Begin
    (
        string name,
        object? details = default,
        object? attachment = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return new Context(Logger, CreateCache(), name).Also(ctx => ctx.Started(new
        {
            meta = details,
            caller = new
            {
                memberName = callerMemberName,
                lineNumber = callerLineNumber,
                filePath = Path.GetFileName(callerFilePath)
            }
        }, attachment));
    }

    public class Context : IDisposable, IEnumerable<IMemoryCache>
    {
        private static readonly IEnumerable<string> Finalizers = new[]
        {
            nameof(UnitOfWorkStatus.Completed),
            nameof(UnitOfWorkStatus.Canceled),
            nameof(UnitOfWorkStatus.Faulted)
        };

        public Context(ILogger logger, IMemoryCache cache, string name)
        {
            Logger = logger;
            cache.Set(nameof(UnitOfWork), name);
            cache.Set(nameof(OneTimeData), OneTimeData);
            Scope = AsyncScope.Push(cache);
        }

        private ILogger Logger { get; }

        private IDisposable Scope { get; }

        public IDictionary<string, object> OneTimeData { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public void Status(string name, object? details, object? attachment)
        {
            Logger.Log(
                new LogEntry(name, this)
                    .SetItem(nameof(details), details)
                    .SetItem(nameof(attachment), attachment));

            foreach (var disposable in OneTimeData.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            OneTimeData.Clear();
        }

        public void Dispose()
        {
            var cache = this.First();
            var isFinalized = Finalizers.Any(cache.Get<bool>);
            var isCommitted = cache.Get<bool>(nameof(UnitOfWorkStatus.Commit));

            // Finalize when the user hasn't already done that.
            if (!isFinalized)
            {
                if (cache.TryGetValue<Exception>(nameof(Exception), out var exception))
                {
                    this.Faulted(attachment: exception);
                }
                else
                {
                    if (isCommitted)
                    {
                        this.Completed(cache.Get("details"), cache.Get("attachment"));
                    }
                    else
                    {
                        this.Inconclusive();
                    }
                }
            }

            Scope.Dispose();
        }

        public IEnumerator<IMemoryCache> GetEnumerator() => AsyncScope<IMemoryCache>.Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}