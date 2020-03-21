using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class LogLevelFromPropertyNode : LoggerNode
    {
        public string Property { get; set; } = default!;

        public ILogLevelMapper Mapper { get; set; } = default!;

        public override void Invoke(ILogEntry request)
        {
            if (Property is null) throw new InvalidOperationException($"{nameof(Property)} must be set first.");
            if (Mapper is null) throw new InvalidOperationException($"{nameof(Mapper)} must be set first.");
            
            if (!request.TryGetProperty(Names.Default.Level, out _) && request.TryGetProperty(Property, out var property))
            {
                request.Add(Names.Default.Level, Mapper.Map(property.Value), m => m.ProcessWith<EchoNode>());
            }

            InvokeNext(request);
        }
    }

    public interface ILogLevelMapper
    {
        LogLevel Map(object? value);
    }

    public class StringLogLevelMapper : Dictionary<string, LogLevel>, ILogLevelMapper
    {
        public StringLogLevelMapper() : base(SoftString.Comparer) { }

        public LogLevel Map(object? value)
        {
            return
                value is {} && TryGetValue(value.ToString(), out var logLevel)
                    ? logLevel
                    : LogLevel.Information;
        }
    }
}