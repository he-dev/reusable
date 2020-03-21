using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Utilities;
using Reusable.Utilities.JsonNet.Converters;
using JsonSerializer = Reusable.OmniLog.Services.JsonSerializer;

namespace Reusable.OmniLog.Nodes
{
    public class SerializerNode : LoggerNode
    {
        public ISerializer Serializer { get; set; } = new JsonSerializer();
        // {
        //     Settings = new JsonSerializerSettings
        //     {
        //         Formatting = Formatting.None,
        //         ContractResolver = new CamelCasePropertyNamesContractResolver(),
        //         Converters =
        //         {
        //             new StringEnumConverter(),
        //             new SoftStringConverter()
        //         }
        //     }
        // };

        /// <summary>
        /// Gets or sets serializable properties. If empty then all items are scanned.
        /// </summary>
        public override void Invoke(ILogEntry request)
        {
            foreach (var property in request.Where(LogProperty.CanProcess.With(this)).ToList().Where(property => property.Value is {}))
            {
                var json = property.Value is string str ? str : Serializer.Serialize(property.Value!);
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