using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Reusable.Translucent;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Reusable.Teapot
{
    [PublicAPI]
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        private readonly ConcurrentDictionary<Guid, ITeapotServerContext> _serverContexts = new ConcurrentDictionary<Guid, ITeapotServerContext>();

        public TeapotServer(string url)
        {
            var configuration =
                new ConfigurationBuilder()
                    // Tests can use their own urls so let's not use hosting.json but in-memory-collection
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["urls"] = url // <-- this is the only way that works with Kestrel
                    })
                    .Build();

            _host =
                new WebHostBuilder()
                    .UseKestrel()
                    .UseConfiguration(configuration)
                    .ConfigureServices(services =>
                    {
                        // Allows to validate requests as they arrive.
                        services.AddSingleton((RequestAssertDelegate)Assert);
                        // Allows to provide custom responses for each request.
                        services.AddSingleton((ResponseMockDelegate)GetResponseFactory);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<TeapotMiddleware>();
                    })
                    .Build();

            _host.StartAsync().GetAwaiter().GetResult(); // <-- asp.net-core TestServer is doing this too.
        }

        //public Task Task { get; set; } // <-- I think I don't need this anymore...

        // Creates a new server-context that separates api-mocks.
        public ITeapotServerContext BeginScope()
        {
            return _serverContexts.GetOrAdd(Guid.NewGuid(), id => new TeapotServerContext(Disposable.Create(() => _serverContexts.TryRemove(id, out _))));
        }

        private void Assert(RequestCopy requestCopy)
        {
            FindApiMock(requestCopy.Method, requestCopy.Uri)?.Assert(requestCopy);
        }

        private Func<HttpRequest, ResponseMock> GetResponseFactory(HttpMethod method, UriString uri)
        {
            return FindApiMock(method, uri)?.GetResponseFactory();
        }

        // Finds an api-mock that should handle the current request.
        [CanBeNull]
        private ApiMock FindApiMock(HttpMethod method, UriString uri)
        {
            if (_serverContexts.IsEmpty) throw new InvalidOperationException($"Cannot get response without a server-context. Call '{nameof(BeginScope)}' first.");

            var mocks =
                from tc in _serverContexts.Values
                from rm in tc
                where rm.Method == method && rm.Uri == uri
                select rm;

            return mocks.FirstOrDefault();
        }

        public void Dispose()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}