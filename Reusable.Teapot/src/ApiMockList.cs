using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface IApiMockCollection : IEnumerable<ApiMock>, IDisposable
    {
        ApiMock MockApi(HttpMethod method, UriString uri);

        void Assert();
    }
    
    internal class ApiMockCollection : List<ApiMock>, IApiMockCollection
    {
        private readonly IDisposable _disposable;

        public ApiMockCollection(IDisposable disposable)
        {
            _disposable = disposable;
        }

        public ApiMock MockApi(HttpMethod method, UriString uri)
        {
            var mock = new ApiMock(method, uri);
            Add(mock);
            return mock;
        }

//        public void Assert(TeacupRequest teacupRequest)
//        {
//            foreach (var mock in _mocks.Where(m => m.Uri == teacupRequest.Uri))
//            {
//                mock.Assert(teacupRequest);
//            }
//        }

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
            _disposable.Dispose();

            foreach (var mock in this)
            {
                //mock.Dispose();
            }
        }
    }
}