using System.IO;
using Newtonsoft.Json;

namespace Reusable.MQLite
{
    public class JsonBodySerializer : IBodySerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        public JsonBodySerializer(JsonSerializer jsonSerializer = null)
        {
            _jsonSerializer = jsonSerializer ?? new JsonSerializer();
        }

        public byte[] Serialize(object obj)
        {
            using (var memoryStream = new MemoryStream())
            using (var textWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                _jsonSerializer.Serialize(jsonWriter, new object());
                return memoryStream.GetBuffer();
            }
        }

        public object Deserialize(byte[] body)
        {
            using (var memoryStream = new MemoryStream(body))
            using (var streamReader = new StreamReader(memoryStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return _jsonSerializer.Deserialize(jsonTextReader);
            }
        }
    }
}