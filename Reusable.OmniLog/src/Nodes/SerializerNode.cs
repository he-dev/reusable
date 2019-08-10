using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    public class SerializerNode : LoggerNode
    {
        private readonly ISerializer _serializer;

        public SerializerNode(ISerializer serializer) : base(true)
        {
            _serializer = serializer;
        }

        public SerializerNode() : this(new JsonSerializer()) { }

        /// <summary>
        /// Gets or sets serializable properties. If empty then all items are scanned.
        /// </summary>
        public HashSet<string> SerializableItems { get; set; } = new HashSet<string>(SoftString.Comparer);
        
        public static ItemKey<SoftString> CreateRequestItemKey(SoftString name) => new ItemKey<SoftString>(name, LogEntry.Tags.Serializable);

        protected override void InvokeCore(LogEntry request)
        {
            // Process only selected #Serializable properties or all.
            var keys =
                SerializableItems.Any()
                    ? SerializableItems.Select(name => new ItemKey<SoftString>(name, LogEntry.Tags.Serializable))
                    : request.Keys().Where(k => k.Tag.Equals(LogEntry.Tags.Serializable));

            foreach (var (name, tag) in keys.ToList())
            {
                if (request.TryGetItem<object>(name, tag, out var obj))
                {
                    request.SetItem(name, default, _serializer.Serialize(obj));
                    //request.RemoveItem((name, tag.ToString())); // Clean-up the old property.
                }
            }

            Next?.Invoke(request);
        }

        //public static ItemKey<SoftString> CreateItemKey(string propertyName) => (propertyName, LogEntryItemTags.Object);
    }

    public static class LoggerSerializerHelper
    {
        // public static LogEntry Serializable(this LogEntry logEntry, string propertyName, object obj)
        // {
        //     return logEntry.SetItem(propertyName, SerializationNode.LogItemTag, obj);
        // }
    }
}