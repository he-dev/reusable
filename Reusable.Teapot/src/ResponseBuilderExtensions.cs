using System.IO;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    [PublicAPI]
    public static class ResponseBuilderExtensions
    {
        public static IResponseBuilder Once(this IResponseBuilder response, int statusCode, object content)
        {
            return response.Exactly(statusCode, content, 1);
        }

        public static IResponseBuilder Always(this IResponseBuilder response, int statusCode, object content)
        {
            return response.Enqueue(request => new ResponseMock(statusCode, content));
        }

        public static IResponseBuilder Exactly(this IResponseBuilder response, int statusCode, object content, int count)
        {
            var counter = 0;
            return response.Enqueue(request => counter++ < count ? new ResponseMock(statusCode, content) : default);
        }

        public static IResponseBuilder Echo(this IResponseBuilder response)
        {
            return response.Enqueue(request =>
            {
                var requestCopy = new MemoryStream();
                request.Body.Rewind().CopyTo(requestCopy);
                return new ResponseMock(200, requestCopy);
            });
        }
    }
}