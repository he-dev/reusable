using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerSerializer : LoggerMiddleware
    {
        public static readonly string LogItemTag = "Serializable";
        
        public static readonly string SerializableSuffix = ".Serializable";

        private static readonly Regex SerializableSuffixRegex = new Regex($"{Regex.Escape(SerializableSuffix)}$", RegexOptions.Compiled);

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

        protected override void InvokeCore(ILog request)
        {
            var propertyNames =
                SerializableProperties.Any()
                    ? SerializableProperties.AsEnumerable().Select(CreateDataKey)
                    : request.Keys.Where(k => k.EndsWith(SerializableSuffix)).Select(k => k.ToString());

            foreach (var propertyName in propertyNames)
            {
                if (request.TryGetValue(propertyName, out var obj))
                {
                    var actualPropertyName = SerializableSuffixRegex.Replace(propertyName, string.Empty); // Remove the suffix.
                    request.SetItem(actualPropertyName, _serializer.Serialize(obj));
                    request.Remove(propertyName); // Clean-up the old property.
                }
            }

            Next?.Invoke(request);
        }

        public static string CreateDataKey(string propertyName) => $"{propertyName}{SerializableSuffix}";
    }
}