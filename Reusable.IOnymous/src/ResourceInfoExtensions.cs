using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Data;

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

        public static async Task<string> DeserializeTextAsync(this IResourceInfo resource)
        {
            if (!resource.Exists) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resource.Uri.ToString()}'");

            var format = resource.Metadata.Get(Use<IResourceSession>.Scope, x => x.Format);

            // todo - find a cleaner solution; maybe a new comparer for MimeType?
            if (!format.Name.StartsWith("text/"))
            {
                throw new ArgumentException($"Resource must be '{MimeType.Text}' but is '{format}'.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await resource.CopyToAsync(memoryStream);
                using (var streamReader = new StreamReader(memoryStream.Rewind()))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public static async Task<T> DeserializeBinaryAsync<T>(this IResourceInfo resource)
        {
            var format = resource.Metadata.Get(Use<IResourceSession>.Scope, x => x.Format);

            if (format != MimeType.Binary)
            {
                throw new ArgumentException($"Resource must be '{MimeType.Binary}' but is '{format}'.");
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                await resource.CopyToAsync(memoryStream);
                return (T)binaryFormatter.Deserialize(memoryStream.Rewind());
            }
        }

        public static async Task<T> DeserializeJsonAsync<T>(this IResourceInfo resource, JsonSerializer jsonSerializer = null)
        {
            var format = resource.Metadata.Get(Use<IResourceSession>.Scope, x => x.Format);

            if (format != MimeType.Json)
            {
                throw new ArgumentException($"Resource must be '{MimeType.Json}' but is '{format}'.");
            }

            jsonSerializer = jsonSerializer ?? new JsonSerializer();
            using (var memoryStream = new MemoryStream())
            {
                await resource.CopyToAsync(memoryStream);
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