using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    /// <summary>
    /// This class provides methods that make it easier to work with resource streams.
    /// </summary>
    public static class ResourceHelper
    {
        internal static string ExtractMethodName(string memberName)
        {
            return Regex.Match(memberName, @"^(?<method>\w+)Async").Groups["method"].Value;
        }

        /// <summary>
        /// Formats exception message: {ResourceProvider} cannot {METHOD} '{uri}' because {reason}.
        /// </summary>
        internal static string FormatMessage<T>(string memberName, UriString uri, string reason)
        {
            return $"{typeof(T).ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        }     

        // --------

        public static Task<Stream> SerializeTextAsync(string value, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Task.FromResult<Stream>(new MemoryStream(encoding.GetBytes(value)));
        }

        public static Task<Stream> SerializeBinaryAsync(object value)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, value);
            return Task.FromResult<Stream>(memoryStream);
        }

        public static async Task<string> DeserializeTextAsync(Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            using (var reader = new StreamReader(stream.Rewind(), encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static Task<T> DeserializeBinaryAsync<T>(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            return Task.FromResult((T)binaryFormatter.Deserialize(stream.Rewind()));
        }

        public static async Task<Stream> SerializeAsJsonAsync(object value, JsonSerializer jsonSerializer = null)
        {
            jsonSerializer = jsonSerializer ?? new JsonSerializer();

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
        
        public static async Task<T> Deserialize<T>(Stream value, Metadata metadata)
        {
            if (metadata.Resource().Format() == MimeType.Text)
            {
                return (T)(object)await DeserializeTextAsync(value);
            }
            
            if (metadata.Resource().Format() == MimeType.Binary)
            {
                return (T)await DeserializeBinaryAsync<object>(value);
            }
            
            throw DynamicException.Create("Format", $"Unsupported value format: '{metadata.Resource().Format()}'.");
        }
    }
}