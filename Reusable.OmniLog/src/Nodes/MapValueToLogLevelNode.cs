using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class MapValueToLogLevelNode : LoggerNode
    {
        public string Property { get; set; } = default!;

        public IMapObjectToLogLevel Mapper { get; set; } = default!;

        public override void Invoke(ILogEntry request)
        {
            if (Property is null) throw new InvalidOperationException($"{nameof(Property)} must be set first.");
            if (Mapper is null) throw new InvalidOperationException($"{nameof(Mapper)} must be set first.");
            
            if (!request.TryGetProperty(Names.Default.Level, out _) && request.TryGetProperty(Property, out var property))
            {
                request.Add(Names.Default.Level, Mapper.Invoke(property.Value), m => m.ProcessWith<EchoNode>());
            }

            InvokeNext(request);
        }
    }

    public interface IMapObjectToLogLevel
    {
        LogLevel Invoke(object? value);
    }

    public class MapStringToLogLevel : Dictionary<string, LogLevel>, IMapObjectToLogLevel
    {
        public MapStringToLogLevel() : base(SoftString.Comparer) { }

        public LogLevel Invoke(object? value)
        {
            return
                value is {} && TryGetValue(value.ToString(), out var logLevel)
                    ? logLevel
                    : LogLevel.Information;
        }
    }
}