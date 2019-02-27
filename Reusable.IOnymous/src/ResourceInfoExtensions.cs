using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class ResourceInfoExtensions
    {
        #region Async

        public static async Task<MemoryStream> CopyToMemoryStreamAsync(this IResourceInfo resource)
        {
            if (!resource.Exists) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resource.Uri.ToString()}'");
            
            var memoryStream = new MemoryStream();
            await resource.CopyToAsync(memoryStream);
            return memoryStream;
        }

        public static async Task<string> DeserializeTextAsync(this IResourceInfo resourceInfo)
        {
            if (!resourceInfo.Exists) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resourceInfo.Uri.ToString()}'");

            // todo - find a cleaner solution; maybe a new comparer for MimeType?
            if (!resourceInfo.Format.Name.StartsWith("text/"))
            {
                throw new ArgumentException($"Resource must be '{MimeType.Text}' but is '{resourceInfo.Format}'.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await resourceInfo.CopyToAsync(memoryStream);
                using (var streamReader = new StreamReader(memoryStream.Rewind()))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public static async Task<T> DeserializeBinaryAsync<T>(this IResourceInfo resourceInfo)
        {
            if (resourceInfo.Format != MimeType.Binary)
            {
                throw new ArgumentException($"Resource must be '{MimeType.Binary}' but is '{resourceInfo.Format}'.");
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                await resourceInfo.CopyToAsync(memoryStream);
                return (T)binaryFormatter.Deserialize(memoryStream.Rewind());
            }
        }

        public static async Task<T> DeserializeJsonAsync<T>(this IResourceInfo resourceInfo, JsonSerializer jsonSerializer = null)
        {
            if (resourceInfo.Format != MimeType.Json)
            {
                throw new ArgumentException($"Resource must be '{MimeType.Json}' but is '{resourceInfo.Format}'.");
            }

            jsonSerializer = jsonSerializer ?? new JsonSerializer();
            using (var memoryStream = new MemoryStream())
            {
                await resourceInfo.CopyToAsync(memoryStream);
                using (var streamReader = new StreamReader(memoryStream.Rewind()))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        #endregion
    }

    public static class StreamExtensions
    {
        public static T Rewind<T>(this T stream) where T : Stream
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }
    }
}