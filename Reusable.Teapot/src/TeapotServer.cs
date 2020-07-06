using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reusable.Teapot
{
    [PublicAPI]
    public class TeapotServer<TStartup> : IDisposable where TStartup : class, new()
    {
        private readonly IWebHost _host;

        public TeapotServer
        (
            string url,
            Action<IServiceCollection>? configureServices = default
        )
        {
            var configuration =
                new ConfigurationBuilder()
                    // Clients can use their own urls so let's not use hosting.json but the in-memory-collection.
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["urls"] = url // <-- this is the only way that works with Kestrel
                    })
                    .Build();

            _host =
                new WebHostBuilder()
                    .UseKestrel()
                    .UseConfiguration(configuration)
                    .ConfigureServices(configureServices ?? (_ => { }))
                    .UseStartup<TStartup>()
                    .Build();

            _host.StartAsync().GetAwaiter().GetResult(); // <-- asp.net-core TestServer is doing this too.
        }


        public virtual void Dispose()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}