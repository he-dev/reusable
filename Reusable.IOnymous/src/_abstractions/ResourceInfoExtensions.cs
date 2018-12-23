using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class ResourceInfoExtensions
    {
        #region Async

        public static async Task<string> DeserializeStringAsync(this IResourceInfo resourceInfo)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            {
                await resourceInfo.CopyToAsync(memoryStream);
                memoryStream.TryRewind();
                return await streamReader.ReadToEndAsync();
            }
        }

        public static async Task<T> DeserializeObjectAsync<T>(this IResourceInfo resourceInfo)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                await resourceInfo.CopyToAsync(memoryStream);
                //memoryStream.TryRewind();
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        public static async Task<T> DeserializeJsonAsync<T>(this IResourceInfo resourceInfo, JsonSerializer jsonSerializer = null)
        {
            jsonSerializer = jsonSerializer ?? new JsonSerializer();
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                await resourceInfo.CopyToAsync(memoryStream);
                //memoryStream.TryRewind();
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }

        #endregion
    }

    public static class StreamExtensions
    {
        public static T TryRewind<T>(this T stream) where T : Stream
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }
    }
}