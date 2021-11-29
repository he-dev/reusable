using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.Translucent
{
    /// <summary>
    /// This class provides methods that make it easier to work with resource streams.
    /// </summary>
    public static class ResourceHelper___
    {
        public static Task<Stream> SerializeTextAsync(string value, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Task.FromResult<Stream>(new MemoryStream(encoding.GetBytes(value)));
        }

        public static Task<Stream> SerializeBinaryAsync(object value)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, value);
            return Task.FromResult<Stream>(memoryStream);
        }

        public static async Task<string> DeserializeTextAsync(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using (var reader = new StreamReader(stream.Rewind(), encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static Task<object> DeserializeBinaryAsync(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            return binaryFormatter.Deserialize(stream.Rewind()).ToTask();
        }

        public static async Task<Stream> SerializeAsJsonAsync(object value, JsonSerializer? jsonSerializer = null)
        {
            jsonSerializer ??= new JsonSerializer();

            using (var memoryStream = new MemoryStream())
            using (var textWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();

                var copy = new MemoryStream();
                await memoryStream.Rewind().CopyToAsync(copy);

                return copy;
            }
        }
        
        // public static async Task<T> Deserialize<T>(Stream value, IImmutableContainer properties)
        // {
        //     
        //     if (properties.GetItemOrDefault(ResponseProperties.Format, MimeType.None) == MimeType.Plain)
        //     {
        //         return (T)(object)await DeserializeTextAsync(value);
        //     }
        //     
        //     if (properties.GetItemOrDefault(ResponseProperties.Format, MimeType.None) == MimeType.Binary)
        //     {
        //         return (T)await DeserializeBinaryAsync(value);
        //     }
        //     
        //     throw DynamicException.Create("ResourceFormat", $"Unsupported resource format: '{properties.GetItemOrDefault(ResponseProperties.Format, MimeType.None)}'.");
        // }
    }
}