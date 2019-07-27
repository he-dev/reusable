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
    class asdf : IConfiguration
    {
        private readonly IDictionary<string, string> _data;

        private readonly IConfigurationSection UrlConfigurationSection;

        public asdf()
        {
            _data = new Dictionary<string, string>
            {
                ["urls"] = "http://localhost:30002"
            };

            UrlConfigurationSection = new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource()
                {
                    InitialData = _data
                })
            }), "urls");
        }


        public IConfigurationSection GetSection(string key)
        {
            return UrlConfigurationSection;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            yield return new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>
            {
                new MemoryConfigurationProvider(new MemoryConfigurationSource()
                {
                    InitialData = _data
                })
            }), "urls");
        }

        public IChangeToken GetReloadToken()
        {
            return NullChangeToken.Singleton;
        }

        public string this[string key]
        {
            get => _data[key];
            set => throw new NotImplementedException();
        }
    }

    [PublicAPI]
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        private readonly ConcurrentDictionary<Guid, IApiMockCollection> _teacups = new ConcurrentDictionary<Guid, IApiMockCollection>();

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
                        services.AddSingleton((CreateResponseMockDelegate)GetResponseFactory);
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

        public IApiMockCollection BeginScope()
        {
            return _teacups.GetOrAdd(Guid.NewGuid(), id =>
            {
                return new ApiMockCollection(Disposable.Create(() =>
                {
                    if (_teacups.TryRemove(id, out var teacup))
                    {
                        teacup.Dispose();
                    }
                }));
            });
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
            if (_teacups.IsEmpty) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");
            
            var mocks =
                from tc in _teacups.Values
                from rm in tc
                where rm.Method == method && rm.Uri == uri
                select rm;
            
          return  mocks.FirstOrDefault();
        }

        public void Dispose()
        {
            _host.Dispose();
            //_teacup?.Dispose();
        }
    }
}