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
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        private readonly ConcurrentDictionary<Guid, ITeacupContext> _serverContexts = new ConcurrentDictionary<Guid, ITeacupContext>();

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
                        services.AddSingleton((RequestAssertDelegate)Assert);
                        services.AddSingleton((ResponseMockDelegate)GetResponseFactory);
                    })
                    .Configure(app => { app.UseMiddleware<TeapotMiddleware>(); })
                    .Build();

            _host.StartAsync().GetAwaiter().GetResult(); // <-- asp.net-core TestServer is doing this too.
        }

        //public Task Task { get; set; } // <-- I think I don't need this anymore...

        // Creates a new server-context that separates api-mocks.
        public ITeacupContext BeginScope()
        {
            return _serverContexts.GetOrAdd(Guid.NewGuid(), id => new TeacupContext(Disposable.Create(() => _serverContexts.TryRemove(id, out _))));
        }

        private void Assert(RequestCopy? requestCopy)
        {
            FindApiMock(requestCopy.Method, requestCopy.Uri)?.Assert(requestCopy);
        }

        private Func<HttpRequest, ResponseMock> GetResponseFactory(HttpMethod method, string uri)
        {
            return FindApiMock(method, uri)?.GetResponseFactory();
        }

        // Finds an api-mock that should handle the current request.
        private ApiMock? FindApiMock(HttpMethod method, string uri)
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