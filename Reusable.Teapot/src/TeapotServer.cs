using System;
using System.Collections.Generic;
using System.Net;
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

        [CanBeNull]
        private TeacupScope _teacup;

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
                        services.AddSingleton((ResponseDelegate)GetResponseFactory);
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

        public ITeacupScope BeginScope() => (_teacup = new TeacupScope(Disposable.Create(() => _teacup = null)));

        private void Assert(RequestInfo request)
        {
            if (_teacup is null) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");

            _teacup.Assert(request);
        }

        private Func<HttpRequest, ResponseInfo> GetResponseFactory(UriString path, SoftString method)
        {
            if (_teacup is null) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");

            return _teacup.GetResponseFactory(path, method);
        }

        public void Dispose()
        {
            _host.Dispose();
            _teacup?.Dispose();
        }
    }
}