using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class ResourceHelper
    {
        public static (Stream Stream, ResourceFormat Format) CreateStream(object value, Encoding encoding = null)
        {
            // Don't dispose streams. The caller takes care of that.

            switch (value)
            {
                case string str:
                {
                    var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(str));
                    return (memoryStream, ResourceFormat.String);
                }
                default:
                {
                    var binaryFormatter = new BinaryFormatter();
                    var memoryStream = new MemoryStream();
                    binaryFormatter.Serialize(memoryStream, value);
                    return (memoryStream, ResourceFormat.Binary);
                }
            }
        }

        public static object CreateObject(Stream stream, ResourceMetadata metadata)
        {
            if (metadata.TryGetValue(ResourceMetadataKeys.Serializer, out string serializerName))
            {
                if (serializerName == nameof(BinaryFormatter))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return binaryFormatter.Deserialize(stream);
                }

                if (serializerName == nameof(StreamReader))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }

                throw DynamicException.Create("UnsupportedSerializer", $"Cannot deserialize stream because the serializer '{serializerName}' is not supported.");
            }

            throw DynamicException.Create("SerializerNotFound", $"Serializer wasn't specified.");
        }

        // --------

        public static Task<Stream> SerializeStringAsync(string value, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Task.FromResult<Stream>(new MemoryStream(encoding.GetBytes(value)));
        }

        public static Task<Stream> SerializeObjectAsBinaryAsync(object value)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, value);
            return Task.FromResult<Stream>(memoryStream);
        }

        public static Task<Stream> SerializeObjectAsJsonAsync(object value, JsonSerializer jsonSerializer = null)
        {
            jsonSerializer = jsonSerializer ?? new JsonSerializer();

            var memoryStream = new MemoryStream();
            using (var textWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();

                return Task.FromResult<Stream>(memoryStream);
            }
        }
    }
}