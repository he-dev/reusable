using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Translucent;

namespace Reusable.Teapot
{
    // This needs to be copy because it otherwise won't pass the middleware/assert "barrier"
    // between the server and the "client".
    public class RequestCopy
    {
        public UriString Uri { get; set; }

        public HttpMethod Method { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public long? ContentLength { get; set; }

        public Stream Content { get; set; }
    }

    public static class RequestCopyExtensions
    {
        // Deserializes the content of request-copy for further analysis.
        public static JToken DeserializeAsJToken(this RequestCopy request)
        {
            if (request.Headers.TryGetValue("Content-Type", out var contentType) && contentType != MimeType.Json)
            {
                throw new ArgumentOutOfRangeException($"This method can deserialize only {MimeType.Json} content.");
            }

            if (request.ContentLength == 0)
            {
                return default;
            }

            using (var memory = new MemoryStream())
            {
                // You copy it because otherwise it'll get disposed when the request-copy vanishes.
                request.Content.Rewind().CopyTo(memory);

                using (var reader = new StreamReader(memory.Rewind()))
                {
                    var body = reader.ReadToEnd();
                    return JToken.Parse(body);
                }
            }
        }
    }
}