using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerSerializer : LoggerMiddleware
    {
        public static readonly string SerializableSuffix = ".Serializable";

        private readonly ISerializer _serializer;
        private readonly IList<string> _propertyNames;

        public LoggerSerializer(ISerializer serializer, params string[] propertyNames) : base(true)
        {
            _serializer = serializer;
            _propertyNames = propertyNames;
        }

        protected override void InvokeCore(ILog request)
        {
            var propertyNames = _propertyNames.Any() ? _propertyNames : request.Keys.Select(k => k.ToString()).ToList();

            foreach (var propertyName in propertyNames)
            {
                var dataKey = propertyName.EndsWith(SerializableSuffix) ? propertyName : CreateDataKey(propertyName);
                if (request.TryGetValue(dataKey, out var obj))
                {
                    var actualPropertyName = Regex.Replace(propertyName, $"{Regex.Escape(SerializableSuffix)}$", string.Empty);
                    request[actualPropertyName] = _serializer.Serialize(obj);
                    request.Remove(dataKey); // Make sure we won't try to do it again
                }
            }


            Next?.Invoke(request);
        }

        public static string CreateDataKey(string propertyName) => $"{propertyName}{SerializableSuffix}";
    }
}