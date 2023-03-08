using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reusable.Utilities.AspNetCore;
using Reusable.Utilities.AspNetCore.Hosting;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Middleware;
using Reusable.Wiretap.Utilities.AspNetCore;

[assembly: AspMvcViewLocationFormat("/src/Views/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("/src/Views/Shared/{0}.cshtml")]

namespace Reusable.Apps.Server
{
    [UsedImplicitly]
    public class Startup
    {
        //public const string HeaderPrefix = "X-Vault-";       

        private readonly IConfiguration _configuration;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = hostingEnvironment;
            var builder = new ConfigurationBuilder()
                .SetBasePath(_webHostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_webHostEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var loggerFactory =
                LoggerFactory
                    .CreateWith<CompletePipeline>()
                    .Configure<MapSnapshot>(node =>
                    {
                        node.Mappings.Add<HttpRequest>(request => new
                        {
                            Path = request.Path.Value,
                            Host = request.Host.Value,
                            request.ContentLength,
                            request.ContentType,
                            request.Cookies,
                            request.Headers,
                            request.IsHttps,
                            request.Method,
                            request.Protocol,
                            request.QueryString,
                        });
                    })
                    .Product("Reusable.app.Server")
                    .Environment(_webHostEnvironment.EnvironmentName)
                    .Echo<ConsoleConnectorDynamic>()
                    .Echo<NLogChannel>();

            services.AddWiretap
            (
                LoggerPipelines
                    .Complete
                    .Configure<InvokePropertyService>(node =>
                    {
                        node.Services.Add(new AttachProperty("Environment", ));
                        node.Services.Add(new AttachProperty("Product", ));
                    })
                    .Configure<FormatPropertyName>(node =>
                    {
                        node.Mappings.Add(Names.Properties.Unit, "Identifier");
                        node.Mappings.Add(Names.Properties.Correlation, "Scope");
                    })
                    .Configure<Echo>(node =>
                    {
#if DEBUG
                        node.Connectors.Add(new SimpleConsoleRx
                        {
                            Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                        });
#endif
                        node.Connectors.Add(new LogToNLog());
                    })
            );

            // Add framework services.
            services
                .AddMvc();
                //.AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringConverter()); });

            // services
            //     .AddScoped<IFeatureToggle>(_ => new FeatureToggle(FeaturePolicy.AlwaysOff));
            //
            // services
            //     .AddScoped<IFeatureController>(s =>
            //     {
            //         var agent = new FeatureController(s.GetService<IFeatureToggle>());
            //         return new FeatureTelemetry(agent, s.GetService<ILogger<FeatureTelemetry>>());
            //     });

            services.Configure<RazorViewEngineOptions>(options => { options.ViewLocationExpanders.Add(new RelativeViewLocationExpander("src")); });

            services.AddSingleton(_webHostEnvironment.ContentRootFileProvider);

            //services.AddApiVersioning(setupAction =>
            //{
            //    setupAction.ApiVersionReader = new HeaderApiVersionReader(HeaderFieldName.ApiVersion);
            //});;

            services.AddSingleton<IConfiguration>(_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();      


            app.UseWiretap();

            //app.UseWhen(
            //    httpContext => !httpContext.Request.Method.In(new[] { "GET" }, StringComparer.OrdinalIgnoreCase),
            //    appBranch =>
            //    {
            //        var headers = new[]
            //        {
            //            HeaderNames.Product,
            //            HeaderNames.Profile
            //        }
            //        .Select(header => $"X-{header}");

            //        appBranch.UseHeaderValidator(headers);
            //    }
            //);

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            if (env.IsDevelopmentAny())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unhandled fault happened.");
                    });
                });
            }
        }
    }
}