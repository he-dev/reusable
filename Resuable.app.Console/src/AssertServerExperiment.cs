using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Reusable.sdk.Http;

namespace Reusable.Utilities.Http
{
    using static RequestLoggerExtensions;

    public static class Program
    {

        public static async Task Run()
        {
            var server = new RequestLogger("http://localhost:12000");

            var client = TestClient.Create("http://localhost:12000/api", headers => { headers.AcceptJson(); });

            var response = await client.Resource("test").Configure(context =>
            {
                context.Body = new { Greeting = "Hallo" };
            }).PostAsync<JToken>();

            //var hasGreeting = requests.First().HasProperty("$.Greetingg");

            //var request = requests.First();

            //var json = server.Logs.ElementAt(0).ToJson()

            await server.Task;

            // await 
        }
    }

    public static class RequestLoggerExtensions
    {
        public static JToken ToJson(this LogEntry log)
        {
            if (log.Request.ContentLength == 0)
            {
                // This supports the null-pattern.
                return JToken.Parse("{}");
            }

            using (var memory = new MemoryStream())
            {
                // It needs to be copied because otherwise it'll get disposed.
                log.Body.Seek(0, SeekOrigin.Begin);
                log.Body.CopyTo(memory);

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

    public static class JTokenExtensions
    {
        //public static void Assert(this JToken token)
    }

    public interface ITestClient : IRestClient { }

    public class TestClient : ITestClient
    {
        private readonly IRestClient _restClient;

        private TestClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public string BaseUri => _restClient.BaseUri;

        public static ITestClient Create(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
        {
            var restClient = new RestClient(baseUri, configureDefaultRequestHeaders);
            return new TestClient(restClient);
        }

        public Task<T> InvokeAsync<T>(HttpMethodContext context, CancellationToken cancellationToken)
        {
            return _restClient.InvokeAsync<T>(context, cancellationToken);
        }
    }

    // -- Assertions

    public static class RequestInfoExtensions
    {
        public static bool HasProperty(this LogEntry request, string jsonPath)
        {
            //return !(request.Body.SelectToken(jsonPath) is null);
            return false;
        }
    }

    // -- server

    public class RequestLogger : IDisposable
    {
        private readonly ConcurrentBag<LogEntry> _logs = new ConcurrentBag<LogEntry>();

        private readonly IWebHost _host;

        public RequestLogger(string url)
        {
            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .UseRequests(_logs)
                    .UseStartup<RequestLoggerStartup>()
                    .Build();


            Task = Task.Factory.StartNew(async () =>
            {
                await _host.RunAsync();
            });
        }

        public Task Task { get; set; }

        public IEnumerable<LogEntry> Logs => _logs;

        public void Dispose()
        {
            _host.Dispose();
            foreach (var log in _logs)
            {
                log.Body.Dispose();
            }
        }
    }

    public class RequestLoggerStartup
    {
        public RequestLoggerStartup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        private IConfiguration Configuration { get; }

        private IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<RequestLoggerMiddleware>();
        }
    }

    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ConcurrentBag<LogEntry> _requests;

        public RequestLoggerMiddleware(RequestDelegate next, ConcurrentBag<LogEntry> requests)
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
                    //context.Request.Body.Seek(0, SeekOrigin.Begin);
                    await context.Request.Body.CopyToAsync(memory);

                    // Rewind to read from the beginning.
                    _requests.Add(new LogEntry { Request = context.Request, Body = memory });

                    await _next(context);
                    context.Response.StatusCode = StatusCodes.Status202Accepted;
                }

            }
            catch (Exception inner)
            {
                // Not sure what to do...
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //throw;
            }
        }
    }

    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseRequests(this IWebHostBuilder hostBuilder, ConcurrentBag<LogEntry> requests)
        {
            return
                hostBuilder
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(requests);
                    });
        }
    }

    public class LogEntry
    {
        public HttpRequest Request { get; set; }

        public MemoryStream Body { get; set; }
    }
}






