using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    public class SerializationNode : LoggerNode
    {
        public static class LogEntryItemTags
        {
            public static readonly string Serializable = nameof(Serializable);
        }

        private readonly ISerializer _serializer;

        public SerializationNode(ISerializer serializer) : base(true)
        {
            _serializer = serializer;
        }

        public SerializationNode() : this(new JsonSerializer()) { }

        /// <summary>
        /// Gets or sets serializable properties. If empty then all keys are scanned.
        /// </summary>
        public HashSet<string> SerializableProperties { get; set; } = new HashSet<string>(SoftString.Comparer);
        
        public static ItemKey<SoftString> CreateRequestItemKey(SoftString name) => new ItemKey<SoftString>(name, LogEntryItemTags.Serializable);

        protected override void InvokeCore(LogEntry request)
        {
            // Process only selected #Serializable properties or all.
            var keys =
                SerializableProperties.Any()
                    ? SerializableProperties.Select(CreateItemKey)
                    : request.Where(l => l.Key.Tag.Equals(LogEntryItemTags.Serializable)).Select(l => l.Key);

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

        public static ItemKey<SoftString> CreateItemKey(string propertyName) => (propertyName, LogEntryItemTags.Serializable);
    }

    public static class LoggerSerializerHelper
    {
        // public static LogEntry Serializable(this LogEntry logEntry, string propertyName, object obj)
        // {
        //     return logEntry.SetItem(propertyName, SerializationNode.LogItemTag, obj);
        // }
    }
}