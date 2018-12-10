using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Reusable.Teapot.Internal
{
    internal static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseRequests(this IWebHostBuilder hostBuilder, RequestLog requests)
        {
            return
                hostBuilder
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(requests);
                    });
        }
    }
}