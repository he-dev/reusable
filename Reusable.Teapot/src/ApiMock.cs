using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Reusable.Exceptionize;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
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

        public ApiMock ArrangeRequest(Action<IRequestBuilder> configure)
        {
            configure(_request);
            return this;
        }

        public ApiMock ArrangeResponse(Action<IResponseBuilder> configure)
        {
            configure(_response.Clear());
            return this;
        }

        public void Assert(TeacupRequest request = default)
        {
            try
            {
                _request.Assert(request);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("Assert", $"{Method} {Uri}", inner);
            }
        }

        public Func<HttpRequest, ResponseMock> GetResponseFactory()
        {
            return request => _response?.Next(request);
        }
    }
}