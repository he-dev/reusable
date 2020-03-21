using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class DefaultLogLevelNode : LoggerNode
    {
        public string Property { get; set; }

        public ILogLevelMapper Mapper { get; set; }

        public override void Invoke(ILogEntry request)
        {
            if (!request.TryGetProperty(LogProperty.Names.Level, out _) && request.TryGetProperty(Property, out var property))
            {
                request.Add(LogProperty.Names.Level, Mapper.Map(property.Value), m => m.ProcessWith<EchoNode>());
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