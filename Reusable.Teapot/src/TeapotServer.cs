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
using Reusable.IOnymous;

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
                    //.SetBasePath(contentRootPath)
                    //.AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["urls"] = url //"http://localhost:30002"
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
                    .Configure(app =>
                    {
                        //app.UsePathBase(url);
                        app.UseMiddleware<TeapotMiddleware>();
                    })
                    //.UseStartup<TeapotStartup>()
                    .Build();

            Task = _host.StartAsync();
            //_host.RunAsync();//.GetAwaiter().GetResult();
            //_host.StartAsync().GetAwaiter().GetResult();
            //_host.StartAsync().GetAwaiter().GetResult();
        }

        public Task Task { get; set; }

        public ITeapotServerContext BeginScope()
        {
            return _serverContexts.GetOrAdd(Guid.NewGuid(), id => new TeapotServerContext(Disposable.Create(() => _serverContexts.TryRemove(id, out _))));
        }

        private void Assert(TeacupRequest request)
        {
            FindApiMock(request.Method, request.Uri)?.Assert(request);
        }

        private Func<HttpRequest, ResponseMock> GetResponseFactory(HttpMethod method, UriString uri)
        {
            return FindApiMock(method, uri)?.GetResponseFactory();
        }

        [CanBeNull]
        private ApiMock FindApiMock(HttpMethod method, UriString uri)
        {
            if (_serverContexts.IsEmpty) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");

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
            //_teacup?.Dispose();
        }
    }
}