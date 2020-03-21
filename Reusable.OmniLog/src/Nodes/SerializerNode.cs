using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Services;
using Reusable.OmniLog.Utilities;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Serializes every #Serializable item in the log-entry and adds it as #Property.
    /// </summary>
    public class SerializerNode : LoggerNode
    {
        public ISerialize Serialize { get; set; } = new SerializeToJson();

        public override void Invoke(ILogEntry request)
        {
            foreach (var property in request.Where(LogProperty.CanProcess.With(this)).ToList().Where(property => property.Value is {}))
            {
                var json = property.Value is string str ? str : Serialize.Invoke(property.Value!);
                request.Add(property.Name, json, m => m.ProcessWith<EchoNode>());
            }

            InvokeNext(request);
        }
    }

    public static class LoggerSerializerHelper
    {
        // public static LogEntry Serializable(this LogEntry logEntry, string propertyName, object obj)
        // {
        //     return logEntry.SetItem(propertyName, SerializationNode.LogItemTag, obj);
        // }
    }
}