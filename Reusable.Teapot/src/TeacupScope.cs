using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface ITeacupScope : IDisposable
    {
        RequestMock Mock(UriString uri);
    }
    
    internal class TeacupScope : ITeacupScope
    {
        private readonly IList<RequestMock> _mocks = new List<RequestMock>();

        public RequestMock Mock(UriString uri)
        {
            var mock = new RequestMock(uri);
            _mocks.Add(mock);
            return mock;
        }

        public void Assert(RequestInfo request)
        {
            foreach (var mock in _mocks.Where(m => m.Uri == request.Uri))
            {
                mock.Assert(request);
            }
        }

        public Func<HttpRequest, ResponseInfo> GetResponseFactory(UriString uri, SoftString method)
        {
            return _mocks.FirstOrDefault(m => m.Uri == uri)?.GetResponseFactory(method);
        }

        public void Dispose()
        {
            foreach (var mock in _mocks)
            {
                //mock.Dispose();
            }
        }
    }
}