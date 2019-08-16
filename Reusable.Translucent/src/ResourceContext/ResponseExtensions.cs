using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.Translucent
{
    public static class ResponseExtensions
    {
        public static bool Exists(this Response response) => response.StatusCode == ResourceStatusCode.OK;

        #region Async

        // public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Response resource)
        // {
        //     if (!resource.Exists()) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{resource.Uri.ToString()}'");
        //
        //     var memoryStream = new MemoryStream();
        //     await resource.CopyToAsync(memoryStream);
        //     return memoryStream;
        // }

        public static async Task<string> DeserializeTextAsync(this Response response)
        {
            //if (!response.Exists()) throw new InvalidOperationException($"Cannot deserialize a resource that does not exist: '{response.Uri.ToString()}'");

            //if (!response.ContentType.Contains(MimeType.Plain | MimeType.Json | MimeType.Html))
            {
                //throw new ArgumentException($"Resource must be '{MimeType.Plain}' but is '{response.ContentType}'.");
            }

            if (response.Body is Stream stream)
            {
                using (var streamReader = new StreamReader(stream.Rewind()))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
            else
            {
                throw new ArgumentException(paramName: nameof(response), message: $"Cannot deserialize body because it's not a stream.");
            }
        }

//        public static Task<T> DeserializeBinaryAsync<T>(this Response response)
//        {
//            if (response.ContentType != MimeType.Binary)
//            {
//                throw new ArgumentException($"Resource must be '{MimeType.Binary}' but is '{response.ContentType}'.");
//            }
//
//            var binaryFormatter = new BinaryFormatter();
//            return ((T)binaryFormatter.Deserialize(response.Body.Rewind())).ToTask();
//        }
//
//        public static Task<T> DeserializeJsonAsync<T>(this Response response, JsonSerializer jsonSerializer = null)
//        {
//            if (response.ContentType != MimeType.Json)
//            {
//                throw new ArgumentException($"Resource must be '{MimeType.Json}' but is '{response.ContentType}'.");
//            }
//
//            jsonSerializer = jsonSerializer ?? new JsonSerializer();
//            using (var streamReader = new StreamReader(response.Body.Rewind()))
//            using (var jsonTextReader = new JsonTextReader(streamReader))
//            {
//                return jsonSerializer.Deserialize<T>(jsonTextReader).ToTask();
//            }
//        }

        // public static async Task<object> DeserializeAsync(this Response resource)
        // {
        //     using (var memoryStream = new MemoryStream())
        //     {
        //         await resource.CopyToAsync(memoryStream);
        //         return await resource.Properties.GetItemOrDefault(ResponseProperties.DeserializeAsync)(memoryStream);
        //     }
        // }

        #endregion


        public static Task<Stream> CreateBodyStreamAsync(this Response request)
        {
            switch (request.Body)
            {
                case Stream stream: return stream.ToTask();
                case string str: return str.ToStream().ToTask();
                default:
                    throw new ArgumentOutOfRangeException($"Cannot create body stream."); // because {nameof(Request.CreateBodyStreamCallback)} is not set.");
            }
        }
    }
}