using System;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This nodes provides properties for log-entry correlation.
/// </summary>
public class UnitOfWorkCorrelation : LoggerMiddleware
{
    public UnitOfWorkCorrelation() => Id = NewId();

    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid().ToString("N");

    public object Id { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        var correlations = UnitOfWork.Enumerate().Select(x => x.Correlation().Id);
        entry.Push<ITransientProperty>(LogProperty.Names.Correlation(), correlations);
        Next?.Invoke(entry);
    }
}