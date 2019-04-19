using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static void Serialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] Stream stream, [NotNull] T obj)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            using (var textWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
            }
        }

        [NotNull]
        public static T Deserialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] Stream stream)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
