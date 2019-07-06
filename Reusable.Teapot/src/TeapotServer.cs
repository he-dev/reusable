using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    [PublicAPI]
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        [CanBeNull] private TeacupScope _teacup;

        public TeapotServer(string url)
        {
            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton((RequestAssertDelegate)Assert);
                        services.AddSingleton((ResponseDelegate)GetResponseFactory);
                    })
                    .UseStartup<TeapotStartup>()
                    //((.UseServer()
                    //.UseIISIntegration()
//                    .UseKestrel()
//                    .ConfigureKestrel((context, options) =>
//                    {
//                        // Set properties and call methods on options
//                        options.Listen(IPAddress.Any, 62001);
//                    })
                    .Build();
            
            Task = _host.StartAsync();
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