using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerSerialization : LoggerMiddleware
    {
        public static readonly string LogItemTag = "Serializable";
        
        //private static readonly Regex SerializableSuffixRegex = new Regex($"{Regex.Escape(SerializableSuffix)}$", RegexOptions.Compiled);

        private readonly ISerializer _serializer;

        public LoggerSerialization(ISerializer serializer) : base(true)
        {
            _serializer = serializer;
        }

        public LoggerSerialization() : this(new JsonSerializer()) { }

        /// <summary>
        /// Gets or sets serializable properties. If empty then all keys are scanned.
        /// </summary>
        public HashSet<string> SerializableProperties { get; set; } = new HashSet<string>(SoftString.Comparer);

        protected override void InvokeCore(LogEntry request)
        {
            var keys =
                SerializableProperties.Any()
                    ? SerializableProperties.AsEnumerable().Select(CreateItemKey)
                    : request.Where(l => l.Key.Tag.Equals(LogItemTag)).Select(l => l.Key);

            foreach (var (name, tag) in keys.ToList())
            {
                if (request.TryGetItem<object>(name, tag, out var obj))
                {
                    request.SetItem(name, default, _serializer.Serialize(obj));
                    request.RemoveItem((name, tag.ToString())); // Clean-up the old property.
                }
            }

            Next?.Invoke(request);
        }

        public static ItemKey<SoftString> CreateItemKey(string propertyName) => (propertyName, LogItemTag);
    }
    
    public static class LoggerSerializerHelper
    {
        public static LogEntry Serializable(this LogEntry logEntry, string propertyName, object obj)
        {
            return logEntry.SetItem(propertyName, LoggerSerialization.LogItemTag, obj);
        }
    }
}