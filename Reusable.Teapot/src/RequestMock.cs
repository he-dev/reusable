using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public class RequestMock
    {
        public static readonly string AnyMethod = string.Empty;

        private readonly IList<(IRequestBuilder Request, IResponseBuilder Response)> _methods = new List<(IRequestBuilder Request, IResponseBuilder Response)>();

        public RequestMock(UriString uri) => Uri = uri;

        public UriString Uri { get; }

        public RequestMock Arrange(string method, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            if (_methods.Any(m => m.Request.Method == method)) throw new ArgumentException($"Method {method.ToUpper()} has already been added.");

            var request = new RequestBuilder(Uri, method);
            var response = new ResponseBuilder(Uri, method);
            configure(request, response);
            _methods.Add((request, response));
            return this;
        }

        public void Assert(RequestInfo request)
        {
            foreach (var method in _methods.Where(m => m.Request.Method.In(AnyMethod, request.Method)))
            {
                method.Request.Assert(request);
            }
        }

        public void Assert()
        {
            foreach (var method in _methods)
            {
                method.Request.Assert(default);
            }
        }

        public Func<HttpRequest, ResponseInfo> GetResponseFactory(SoftString method)
        {
            var response = _methods.SingleOrDefault(m => m.Response.Method == method).Response;
            return request => response?.Next(request);
        }
    }
    
    public static class RequestMockExtensions
    {
        public static RequestMock ArrangeGet(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("GET", configure);
        }

        public static RequestMock ArrangePost(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("POST", configure);
        }

        public static RequestMock ArrangePut(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("PUT", configure);
        }

        public static RequestMock ArrangeDelete(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("DELETE", configure);
        }

        public static RequestMock ArrangeAny(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange(RequestMock.AnyMethod, configure);
        }
    }
}