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
}