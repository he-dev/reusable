using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This nodes maps properties into log-levels.
    /// </summary>
    public class MapPropertyToLogLevel : LoggerNode
    {
        public string PropertyName { get; set; } = default!;

        public IMapObjectToLogLevel Mapper { get; set; } = default!;

        public override void Invoke(ILogEntry request)
        {
            if (PropertyName is null) throw new InvalidOperationException($"{nameof(PropertyName)} must be set first.");
            if (Mapper is null) throw new InvalidOperationException($"{nameof(Mapper)} must be set first.");
            
            if (!request.TryGetProperty(Names.Properties.Level, out _) && request.TryGetProperty(PropertyName, out var property))
            {
                request.Push(Names.Properties.Level, Mapper.Invoke(property.Value), m => m.ProcessWith<Echo>());
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