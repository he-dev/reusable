using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerSerializer : LoggerMiddleware
    {
        public static readonly string LogItemTag = "Serializable";
        
        //private static readonly Regex SerializableSuffixRegex = new Regex($"{Regex.Escape(SerializableSuffix)}$", RegexOptions.Compiled);

        private readonly ISerializer _serializer;

        public LoggerSerializer(ISerializer serializer) : base(true)
        {
            _serializer = serializer;
        }

        public LoggerSerializer() : this(new JsonSerializer()) { }

        /// <summary>
        /// Gets or sets serializable properties. If empty then all keys are scanned.
        /// </summary>
        public HashSet<string> SerializableProperties { get; set; } = new HashSet<string>(SoftString.Comparer);

        protected override void InvokeCore(Log request)
        {
            var keys =
                SerializableProperties.Any()
                    ? SerializableProperties.AsEnumerable().Select(CreateItemKey)
                    : request.Where(l => l.Key.Tag.Equals(LogItemTag)).Select(l => l.Key);

            foreach (var (name, tag) in keys.ToList())
            {
                if (request.TryGetItem<object>((name, tag.ToString()), out var obj))
                {
                    request.SetItem((name, default), _serializer.Serialize(obj));
                    request.RemoveItem((name, tag.ToString())); // Clean-up the old property.
                }
            }

            Next?.Invoke(request);
        }

        public static ItemKey<SoftString> CreateItemKey(string propertyName) => (propertyName, LogItemTag);
    }
    
    public static class LoggerSerializerHelper
    {
        public static Log Serializable(this Log log, string propertyName, object obj)
        {
            return log.SetItem((propertyName, LoggerSerializer.LogItemTag), obj);
        }
    }
}