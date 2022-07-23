using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public class LoggerScope : LoggerMiddleware, ILoggerScope
{
    public LoggerScope(IEnumerable<ILoggerMiddleware> middleware, IDisposable disposer)
    {
        Disposer = disposer;
        this.Join(middleware);
    }

    private IDisposable Disposer { get; }

    public IEnumerable<ILoggerScope> Parents => AsyncScope<ILoggerScope>.Current.Enumerate().Select(s => s.Value);

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public override void Invoke(ILogEntry entry) => Next?.Invoke(entry);

    public void Dispose()
    {
        // Skip self or otherwise it'll be recursive.
        this.EnumerateNext<ILoggerMiddleware>(includeSelf: false).Dispose();
        Properties.Dispose();
        Disposer.Dispose();
    }
}