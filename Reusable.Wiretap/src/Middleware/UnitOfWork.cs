using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

public class UnitOfWork : LoggerMiddleware
{
    public UnitOfWork(IEnumerable<Func<ILoggerMiddleware>> builders) => Builders = builders;

    private IEnumerable<Func<ILoggerMiddleware>> Builders { get; }

    public static Item? Current => AsyncScope<Item>.Current?.Value;

    /// <summary>
    /// Enumerates unit-of-work chain.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Item> Enumerate() => AsyncScope<Item>.Current.Enumerate().Reverse().Select(s => s.Value);

    public Item Push(ILogger logger, string name)
    {
        var unitOfWork = AsyncScope<Item>.Push(disposer => new Item(Builders.Invoke())
        {
            Logger = logger,
            Name = name,
            Disposer = disposer,
        }).Value;

        logger.Log(unitOfWork.Layer(Telemetry.Collect).UnitOfWork(name).Started());

        return unitOfWork;
    }

    public override void Invoke(ILogEntry entry)
    {
        Current?.Invoke(entry);
        Next?.Invoke(entry);
    }

    public delegate Telemetry.Builder<ITelemetryLayer> LayerFunc(Telemetry.Builder<ITelemetry> telemetry);

    public class Item : LoggerMiddleware, IDisposable
    {
        internal Item(IEnumerable<ILoggerMiddleware> middleware) => this.Join(middleware);

        internal IDisposable Disposer { get; init; } = default!;

        internal ILogger Logger { get; init; } = default!;

        internal string Name { get; init; } = default!;

        public LayerFunc Layer { get; set; } = t => t.Application();

        public Exception? Exception { get; set; }

        public override void Invoke(ILogEntry entry) => Next?.Invoke(entry);

        public void Dispose()
        {
            Logger.Log(Layer(Telemetry.Collect).UnitOfWork(Name).Let(unitOfWork =>
            {
                return Exception switch
                {
                    { } e => unitOfWork.Faulted(e), _ => unitOfWork.Completed()
                };
            }));

            // Skip self or otherwise it'll be recursive.
            this.EnumerateNext<ILoggerMiddleware>(includeSelf: false).Dispose();
            Disposer.Dispose();
        }
    }
}