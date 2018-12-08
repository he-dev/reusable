using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Reusable.Tests2
{
    public class RequestLogger : IDisposable
    {
        private readonly ConcurrentDictionary<PathString, List<RequestInfo>> _requests;

        private readonly IWebHost _host;

        public RequestLogger(string url)
        {
            _requests = new ConcurrentDictionary<PathString, List<RequestInfo>>();

            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .UseRequests(_requests)
                    .UseStartup<RequestLoggerStartup>()
                    .Build();


            Task = Task.Factory.StartNew(async () =>
            {
                await _host.RunAsync();
            });
        }

        public Task Task { get; set; }

        [NotNull, ItemNotNull]
        public IEnumerable<RequestInfo> this[PathString path] => _requests[path];

        public void Dispose()
        {
            _host.Dispose();
            foreach (var request in _requests.SelectMany(r => r.Value))
            {
                request.Body?.Dispose();
            }
        }
    }

    public class RequestLoggerStartup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<RequestLoggerMiddleware>();
        }
    }

    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ConcurrentDictionary<PathString, List<RequestInfo>> _requests;

        public RequestLoggerMiddleware(RequestDelegate next, ConcurrentDictionary<PathString, List<RequestInfo>> requests)
        {
            _next = next;
            _requests = requests;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // We'll need this later so don't dispose.
                var memory = new MemoryStream();
                {
                    // It needs to be copied because otherwise it'll get disposed.
                    await context.Request.Body.CopyToAsync(memory);

                    var request = new RequestInfo
                    {
                        Path = context.Request.Path,
                        ContentLength = context.Request.ContentLength,
                        // There is no copy-constructor.
                        Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                        Body = memory
                    };

                    _requests.AddOrUpdate
                    (
                        context.Request.Path, 
                        path => new List<RequestInfo> { request }, 
                        (path, requests) =>
                        {
                            requests.Add(request);
                            return requests;
                        });

                    await _next(context);
                    context.Response.StatusCode = StatusCodes.Status418ImATeapot;
                }

            }
            catch (Exception inner)
            {
                // Not sure what to do... throw or not?
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //throw;
            }
        }
    }

    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseRequests(this IWebHostBuilder hostBuilder, ConcurrentDictionary<PathString, List<RequestInfo>> requests)
        {
            return
                hostBuilder
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(requests);
                    });
        }
    }

    public class RequestInfo
    {
        public long? ContentLength { get; set; }

        public PathString Path { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public MemoryStream Body { get; set; }
    }

    public static class RequestLoggerExtensions
    {
        public static JToken ToJson(this RequestInfo info)
        {
            if (info.ContentLength == 0)
            {
                // This supports the null-pattern.
                return JToken.Parse("{}");
            }

            using (var memory = new MemoryStream())
            {
                // It needs to be copied because otherwise it'll get disposed.
                info.Body.Seek(0, SeekOrigin.Begin);
                info.Body.CopyTo(memory);

                // Rewind to read from the beginning.
                memory.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memory))
                {
                    var body = reader.ReadToEnd();
                    return JToken.Parse(body);
                }
            }
        }
    }
}
