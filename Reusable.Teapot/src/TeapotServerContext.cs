using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface ITeapotServerContext : IEnumerable<ApiMock>, IDisposable
    {
        ApiMock MockApi(HttpMethod method, UriString uri);

        void Assert();
    }

    internal class TeapotServerContext : List<ApiMock>, ITeapotServerContext
    {
        private readonly IDisposable _disposer;

        public TeapotServerContext(IDisposable disposer)
        {
            _disposer = disposer;
        }

        public ApiMock MockApi(HttpMethod method, UriString uri)
        {
            var mock = new ApiMock(method, uri);
            Add(mock);
            return mock;
        }

        public void Assert()
        {
            foreach (var apiMock in this)
            {
                apiMock.Assert();
            }
        }

        public Func<HttpRequest, ResponseMock> GetResponseFactory(HttpMethod method, UriString uri)
        {
            return this.FirstOrDefault(m => m.Method == method && m.Uri == uri)?.GetResponseFactory();
        }

        public void Dispose()
        {
            _disposer.Dispose();
        }
    }

    public static class TeapotServerContextExtensions
    {
        public static ApiMock MockGet(this ITeapotServerContext context, string uri, Action<IRequestBuilder> configureRequest)
        {
            return
                context
                    .MockApi(HttpMethod.Get, uri)
                    .ArrangeRequest(configureRequest);
        }
        
        public static ApiMock MockPost(this ITeapotServerContext context, string uri, Action<IRequestBuilder> configureRequest)
        {
            return
                context
                    .MockApi(HttpMethod.Post, uri)
                    .ArrangeRequest(configureRequest);
        }
        
        public static ApiMock MockPut(this ITeapotServerContext context, string uri, Action<IRequestBuilder> configureRequest)
        {
            return
                context
                    .MockApi(HttpMethod.Put, uri)
                    .ArrangeRequest(configureRequest);
        }
        
        public static ApiMock MockDelete(this ITeapotServerContext context, string uri, Action<IRequestBuilder> configureRequest)
        {
            return
                context
                    .MockApi(HttpMethod.Delete, uri)
                    .ArrangeRequest(configureRequest);
        }
    }
}