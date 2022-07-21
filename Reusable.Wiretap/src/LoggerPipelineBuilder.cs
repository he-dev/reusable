using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Nodes.Scopeable;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

[PublicAPI]
public class LoggerPipelineBuilder : ILoggerPipelineBuilder
{
    public IEnumerable<AttachProperty> Properties { get; set; } = Enumerable.Empty<AttachProperty>();

    public IEnumerable<SnapshotMapping> Mappings { get; set; } = Enumerable.Empty<SnapshotMapping>();

    public IEnumerable<FilterEntries> Filters { get; set; } = Enumerable.Empty<FilterEntries>();

    public IEnumerable<SerializeProperties> Serializers { get; set; } = new[]
    {
        new SerializeProperties
        {
            Serialize = new SerializeToJson()
        }
    };

    public IEnumerable<AttachProperty> Fallbacks { get; set; } = new AttachProperty[]
    {
        //new Attach<IRegularProperty>(LogProperty.Names.Level(), LogLevel.Information),
        new Attach<IRegularProperty>(LogProperty.Names.Layer(), "None"),
        new Attach<IRegularProperty>(LogProperty.Names.Category(), "None"),
        new Attach<IRegularProperty>(LogProperty.Names.Tag(), "None"),
        new Attach<IRegularProperty>(LogProperty.Names.Snapshot(), "Empty"),
    };

    public IEnumerable<FormatPropertyName> Formattings { get; set; } = new FormatPropertyName[]
    {
        new Capitalize(),
    };

    public IEnumerable<Channel> Channels { get; set; } = Enumerable.Empty<Channel>();

    private IEnumerable<ILoggerNode> Nodes(string loggerName)
    {
        yield return new Attach<IRegularProperty>(LogProperty.Names.Logger(), loggerName);

        foreach (var node in Properties) yield return node;

        yield return new AttachTimestamp<DateTimeUtc>();

        yield return new GuessProperty();
        yield return new GuessMessage();
        yield return new GuessEnum();
        yield return new GuessException();

        foreach (var node in Mappings) yield return node;
        foreach (var node in Filters) yield return node;

        yield return new ActivateScope
        {
            () => new Correlation(),
            () => new ScopeElapsed(),
            () => new ScopeBuffer(),
        };

        yield return new SplitSnapshots();
        yield return new InferUnitOfWorkState();
        yield return new InferUnitOfWorkResult();

        foreach (var node in Serializers) yield return node;

        // yield return new InferLogLevelFromException();
        //
        // yield return new InferLogLevelFromLayer(new Dictionary<string, LogLevel>(SoftString.Comparer)
        // {
        //     [nameof(TelemetryLayers.Presentation)] = LogLevel.Trace,
        //     [nameof(TelemetryLayers.Application)] = LogLevel.Debug,
        //     [nameof(TelemetryLayers.Business)] = LogLevel.Information,
        //     [nameof(TelemetryLayers.Persistence)] = LogLevel.Trace,
        // });

        foreach (var node in Fallbacks) yield return node;
        foreach (var node in Formattings) yield return node;
        foreach (var node in Channels) yield return node;
    }

    public ILoggerNode Build(string loggerName) => Nodes(loggerName).Join().First();
}