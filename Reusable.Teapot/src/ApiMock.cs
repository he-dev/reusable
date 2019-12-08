using System;
using System.Net.Http;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Translucent;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Reusable.Teapot
{
    /// <summary>
    /// Represents a single api-mock.
    /// </summary>
    public class ApiMock
    {
        private readonly IRequestAssert _request;
        private readonly IResponseFactory _responseFactory;

        public ApiMock(HttpMethod method, UriString uri)
        {
            Method = method;
            Uri = uri;

            _request = new RequestAssert();
            _responseFactory = new ResponseFactory().Always(200, new { Message = "OK" }, MimeType.Json);
        }

        public HttpMethod Method { get; }

        public UriString Uri { get; }

        // Allows to configure request asserts.
        public ApiMock ArrangeRequest(Action<IRequestAssert> configure)
        {
            configure(_request);
            return this;
        }

        // Allows to configure responses.
        public ApiMock ArrangeResponse(Action<IResponseFactory> configure)
        {
            configure(_responseFactory.Clear());
            return this;
        }

        // Validates the request either as it arrives (not-null) or afterwards (null).
        public void Assert(RequestCopy? requestCopy = default)
        {
            try
            {
                _request.Assert(requestCopy);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("Assert", $"Could not assert '{Method}/{Uri}'.", inner);
            }
        }

        // Tries to get the nest response.
        public Func<HttpRequest, ResponseMock> GetResponseFactory()
        {
            return request => _responseFactory.Next(request);
        }
    }
}