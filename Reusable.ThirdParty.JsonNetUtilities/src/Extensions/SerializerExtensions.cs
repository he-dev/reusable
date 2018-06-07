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
    public static class SerializerExtensions
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
                return memoryStream.GetBuffer();
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
