using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
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

            _request = new RequestBuilder(method, uri);
            _response = new ResponseBuilder(method, uri).Always(200, new { Message = "OK" });
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

        public void Assert(TeacupRequest request)
        {
            _request.Assert(request);
        }

        public void Assert()
        {
            _request.Assert(default);
            //_response.Assert(default);
        }

        public Func<HttpRequest, ResponseMock> GetResponseFactory()
        {
            return request => _response?.Next(request);
        }
    }

    public static class RequestMockExtensions
    {
//        public static RequestMock ArrangeRequest(this RequestMock requestMock, Action<IRequestBuilder> configure)
//        {
//            return requestMock.Arrange("GET", configure);
//        }
//        
//        public static RequestMock ArrangeResponse(this RequestMock requestMock, Action<IResponseBuilder> configure)
//        {
//            return requestMock.Arrange("GET", configure);
//        }
    }
}