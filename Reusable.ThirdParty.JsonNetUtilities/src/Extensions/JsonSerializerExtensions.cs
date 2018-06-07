using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Extensions
{
    public static class JsonSerializerExtensions
    {
        [NotNull]
        public static byte[] SerializeToBytes<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] T obj)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            using (var memoryStream = new MemoryStream())
            using (var textWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        [NotNull]
        public static T Deserialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] byte[] body)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (body == null) throw new ArgumentNullException(nameof(body));

            using (var memoryStream = new MemoryStream(body))
            using (var streamReader = new StreamReader(memoryStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
