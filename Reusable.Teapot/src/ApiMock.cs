using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Reusable.Exceptionize;
using Reusable.Translucent;

namespace Reusable.Teapot
{
    // Represents a single api-mock.
    public class ApiMock
    {
        private readonly IRequestBuilder _request;
        private readonly IResponseBuilder _response;

        public ApiMock(HttpMethod method, UriString uri)
        {
            Method = method;
            Uri = uri;

            _request = new RequestBuilder();
            _response = new ResponseBuilder().Always(200, new { Message = "OK" }, MimeType.Json);
        }

        public HttpMethod Method { get; }

        public UriString Uri { get; }

        // Allows to configure request asserts.
        public ApiMock ArrangeRequest(Action<IRequestBuilder> configure)
        {
            configure(_request);
            return this;
        }

        // Allows to configure responses.
        public ApiMock ArrangeResponse(Action<IResponseBuilder> configure)
        {
            configure(_response.Clear());
            return this;
        }

        // Validates the request either as it arrives (not-null) or afterwards (null).
        public void Assert(RequestCopy requestCopy = default)
        {
            try
            {
                _request.Assert(requestCopy);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("Assert", $"{Method} {Uri}", inner);
            }
        }

        // Tries to get the nest response.
        public Func<HttpRequest, ResponseMock> GetResponseFactory()
        {
            return request => _response?.Next(request);
        }
    }
}