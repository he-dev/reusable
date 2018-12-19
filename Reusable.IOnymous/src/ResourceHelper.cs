using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class ResourceHelper
    {
        public static (Stream Stream, ResourceMetadata Metadata) CreateStream(object value, Encoding encoding = null)
        {
            // Don't dispose streams. The caller takes care of that.

            switch (value)
            {
                case string s:
                    var streamReader = s.ToStreamReader(encoding ?? Encoding.UTF8);
                    return (streamReader.BaseStream, ResourceMetadata.Empty.Add(ResourceMetadataKeys.Serializer, nameof(StreamReader)));
                default:
                    var binaryFormatter = new BinaryFormatter();
                    var memoryStream = new MemoryStream();
                    binaryFormatter.Serialize(memoryStream, value);
                    return (memoryStream, ResourceMetadata.Empty.Add(ResourceMetadataKeys.Serializer, nameof(BinaryFormatter)));
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
    }
}