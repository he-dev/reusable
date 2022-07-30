using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap;

[PublicAPI]
public class TelemetryLoggerBuilder : ILoggerBuilder
{
    public List<ILoggerMiddleware> Settings { get; set; } = new()
    {
        new AttachTimestamp(new DateTimeUtc())
    };
    
    public List<Func<ILoggerMiddleware>> UnitOfWorkFeatures { get; set; } = new()
    {
        () => new UnitOfWorkCorrelation(),
        () => new UnitOfWorkElapsed(),
        () => new UnitOfWorkBuffer()
    };

    public List<SnapshotMapping> Mappings { get; set; } = new();

    public List<FormatPropertyName> Formattings { get; set; } = new()
    {
        new Capitalize(),
    };

    public List<FilterEntries> Filters { get; set; } = new();

    public List<SerializeProperty> Serializers { get; set; } = new()
    {
        new SerializeToJson("Correlation"),
        new SerializeToJson("Snapshot"),
        new SerializeTimeSpan("Elapsed")
    };

    public List<IChannel> Channels { get; set; } = new();

    private IEnumerable<ILoggerMiddleware> Middleware()
    {
        foreach (var node in Settings) yield return node;

        yield return new UnitOfWork(UnitOfWorkFeatures);
        yield return new SplitSnapshots();

        foreach (var node in Mappings) yield return node;
        foreach (var node in Formattings) yield return node;
        foreach (var node in Filters) yield return node;
        foreach (var node in Serializers) yield return node;
        foreach (var node in Channels) yield return node;
    }

    public ILogger Build(string name) => new Logger(name).Also(l => l.Join(Middleware()));
}