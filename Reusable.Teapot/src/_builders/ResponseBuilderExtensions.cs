using System.IO;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent;

namespace Reusable.Teapot
{
    [PublicAPI]
    public static class ResponseBuilderExtensions
    {
        public static IResponseBuilder Once(this IResponseBuilder response, int statusCode, object content, string contentType = default)
        {
            return response.Exactly(statusCode, content, content.DetermineContentType(contentType), 1);
        }

        public static IResponseBuilder Always(this IResponseBuilder response, int statusCode, object content, string contentType = default)
        {
            return response.Enqueue(request => new ResponseMock(statusCode, content, content.DetermineContentType(contentType)));
        }

        public static IResponseBuilder Exactly(this IResponseBuilder response, int statusCode, object content, string contentType, int count)
        {
            var counter = 0;
            return response.Enqueue(request => counter++ < count ? new ResponseMock(statusCode, content, contentType) : default);
        }

        // Forwards request to response.
        public static IResponseBuilder Echo(this IResponseBuilder response)
        {
            return response.Enqueue(request =>
            {
                var requestCopy = new MemoryStream();
                request.Body.Rewind().CopyTo(requestCopy);
                return new ResponseMock(200, requestCopy, MimeType.Binary);
            });
        }

        private static string DetermineContentType(this object content, string contentType)
        {
            return contentType ?? (content is string ? MimeType.Plain : MimeType.Json);
        }
    }
}