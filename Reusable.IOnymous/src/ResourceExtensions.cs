using System;
using System.IO;
using System.Linq.Custom;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class ResourceExtensions
    {
        #region Async

        public static async Task<MemoryStream> CopyToMemoryStreamAsync(this IResource resource)
        {
            if (!resource.Exists) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resource.Uri.ToString()}'");

            var memoryStream = new MemoryStream();
            await resource.CopyToAsync(memoryStream);
            return memoryStream;
        }

        public static async Task<string> DeserializeTextAsync(this IResource resource)
        {
            if (!resource.Exists) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resource.Uri.ToString()}'");

            var format = resource.Properties.GetItemOrDefault(Resource.Property.Format);

            if (!format.Contains(MimeType.Text | MimeType.Html))
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

        public static async Task<T> DeserializeBinaryAsync<T>(this IResource resource)
        {
            if (resource.Format != MimeType.Binary)
            {
                throw new ArgumentException($"Resource must be '{MimeType.Binary}' but is '{resource.Format}'.");
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                await resource.CopyToAsync(memoryStream);
                return (T)binaryFormatter.Deserialize(memoryStream.Rewind());
            }
        }

        public static async Task<T> DeserializeJsonAsync<T>(this IResource resource, JsonSerializer jsonSerializer = null)
        {
            var format = resource.Properties.GetItemOrDefault(Resource.Property.Format);

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