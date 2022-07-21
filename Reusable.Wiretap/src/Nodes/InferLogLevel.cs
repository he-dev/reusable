using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public abstract class InferLogLevel : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        if (Infer(entry) is { } level and > LogLevel.None)
        {
            entry.Push(new LogProperty<IRegularProperty>(LogProperty.Names.Level(), level));
        }

        Next?.Invoke(entry);
    }

    protected abstract LogLevel? Infer(ILogEntry entry);
}

public class InferLogLevelFromLayer : InferLogLevel
{
    public InferLogLevelFromLayer(IDictionary<string, LogLevel> mappings)
    {
        Mappings = mappings;
    }

    private IDictionary<string, LogLevel> Mappings { get; }

    protected override LogLevel? Infer(ILogEntry entry)
    {
        var canInfer = entry[LogProperty.Names.Level()].Value is not LogLevel or <= LogLevel.None;
        var layer = entry[LogProperty.Names.Layer()].Value as string ?? string.Empty;
        var containsMapping = Mappings.TryGetValue(layer, out var level);

        return
            canInfer && containsMapping
                ? level
                : default;
    }
}

public class InferLogLevelFromException : InferLogLevel
{
    protected override LogLevel? Infer(ILogEntry entry)
    {
        var canInfer = entry[LogProperty.Names.Level()].Value is not LogLevel or <= LogLevel.None;
        var containsException = entry[LogProperty.Names.Exception()].Value is Exception;

        return
            canInfer && containsException
                ? LogLevel.Error
                : default;
    }
}