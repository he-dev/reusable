using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Translucent;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Reusable.Teapot
{
    [PublicAPI]
    public class AssertServer : TeapotServer<Startup>
    {
        private readonly AssertHelper _helper;

        public AssertServer(string url) : this(url, new AssertHelper()) { }

        private AssertServer(string url, AssertHelper helper) : base(url, configureServices: services =>
        {
            services.AddSingleton((RequestAssertDelegate)helper.Assert);
            services.AddSingleton((ResponseMockDelegate)helper.GetResponseFactory);
        })
        {
            _helper = helper;
        }

        //public Task Task { get; set; } // <-- I think I don't need this anymore...

        // Creates a new server-context that separates api-mocks.
        public ITeacupContext BeginScope() => _helper.BeginScope();

        private class AssertHelper
        {
            private readonly ConcurrentDictionary<Guid, ITeacupContext> _teacups = new ConcurrentDictionary<Guid, ITeacupContext>();

            public ITeacupContext BeginScope()
            {
                return _teacups.GetOrAdd(Guid.NewGuid(), id => new TeacupContext(Disposable.Create(() => _teacups.TryRemove(id, out _))));
            }

            public void Assert(RequestCopy? requestCopy)
            {
                FindApiMock(requestCopy.Method, requestCopy.Uri)?.Assert(requestCopy);
            }

            public Func<HttpRequest, ResponseMock> GetResponseFactory(HttpMethod method, string uri)
            {
                return FindApiMock(method, uri)?.GetResponseFactory();
            }

            // Finds an api-mock that should handle the current request.
            private ApiMock? FindApiMock(HttpMethod method, string uri)
            {
                if (_teacups.IsEmpty) throw new InvalidOperationException($"Cannot get response without a server-context. Call '{nameof(BeginScope)}' first.");

                var mocks =
                    from tc in _teacups.Values
                    from rm in tc
                    where rm.Method == method && rm.Uri == uri
                    select rm;

                return mocks.FirstOrDefault();
            }
        }
    }
}