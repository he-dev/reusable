using System.IO;
using JetBrains.Annotations;
using Reusable.Marbles.Data;
using Reusable.Marbles.Extensions;

namespace Reusable.Teapot
{
    [PublicAPI]
    public static class ResponseBuilderExtensions
    {
        public static IResponseFactory Once(this IResponseFactory response, int statusCode, object content, string? contentType = default)
        {
            return response.Exactly(statusCode, content, content.DetermineContentType(contentType), 1);
        }

        public static IResponseFactory Always(this IResponseFactory response, int statusCode, object content, string? contentType = default)
        {
            return response.Enqueue(request => new ResponseMock(statusCode, content, content.DetermineContentType(contentType)));
        }

        public static IResponseFactory Exactly(this IResponseFactory response, int statusCode, object content, string contentType, int count)
        {
            var counter = 0;
            return response.Enqueue(request => counter++ < count ? new ResponseMock(statusCode, content, contentType) : default);
        }

        // Forwards request to response.
        public static IResponseFactory Echo(this IResponseFactory response)
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