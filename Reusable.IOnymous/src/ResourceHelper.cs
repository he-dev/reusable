using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class ResourceHelper
    {
        internal static string ExtractMethodName(string memberName)
        {
            return Regex.Match(memberName, @"^(?<method>\w+)Async").Groups["method"].Value;
        }

        /// <summary>
        /// Formats exception message: {ResourceProvider} cannot {METHOD} '{uri}' because {reason}.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="uri"></param>
        /// <param name="reason"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static string FormatMessage<T>(string memberName, UriString uri, string reason)
        {
            return $"{typeof(T).ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        }

//        [Obsolete("Use SerializeX")]
//        public static (Stream Stream, MimeType Format) CreateStream(object value, Encoding encoding = null)
//        {
//            // Don't dispose streams. The caller takes care of that.
//
//            switch (value)
//            {
//                case string str:
//                {
//                    var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(str));
//                    return (memoryStream, MimeType.Text);
//                }
//                default:
//                {
//                    var binaryFormatter = new BinaryFormatter();
//                    var memoryStream = new MemoryStream();
//                    binaryFormatter.Serialize(memoryStream, value);
//                    return (memoryStream, MimeType.Binary);
//                }
//            }
//        }        

        // --------

        public static Task<Stream> SerializeAsTextAsync(string value, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Task.FromResult<Stream>(new MemoryStream(encoding.GetBytes(value)));
        }

        public static Task<Stream> SerializeAsBinaryAsync(object value)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, value);
            return Task.FromResult<Stream>(memoryStream);
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
    }
}