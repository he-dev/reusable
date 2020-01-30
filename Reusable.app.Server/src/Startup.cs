using System.Linq;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Apps.Server.Json;
using Reusable.Beaver;
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.Rx.ConsoleRenderers;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions;
using Reusable.OmniLog.Services;
using Reusable.Utilities.AspNetCore;
using Reusable.Utilities.AspNetCore.Hosting;
using Reusable.Utilities.NLog.LayoutRenderers;

[assembly: AspMvcViewLocationFormat("/src/Views/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("/src/Views/Shared/{0}.cshtml")]

namespace Reusable.Apps.Server
{
    [UsedImplicitly]
    public class Startup
    {
        //public const string HeaderPrefix = "X-Vault-";       

        private readonly IConfiguration _configuration;

        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            var builder = new ConfigurationBuilder()
                .SetBasePath(_hostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_hostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SmartPropertiesLayoutRenderer.Register();

            services.AddOmniLog(builder =>
            {
                builder
                    .UseStopwatch()
                    .UseService
                    (
                        new Constant("Environment", _hostingEnvironment.EnvironmentName),
                        new Constant("Product", "Reusable.app.Server"),
                        new Timestamp<DateTimeUtc>()
                    )
                    .UseDelegate()
                    .UseScope()
                    .UseBuilder()
                    .UseDestructure()
                    .UseObjectMapper()
                    .UseSerializer()
                    .UsePropertyMapper
                    (
                        (LogEntry.Names.SnapshotName, "Identifier")
                    )
                    .UseFallback((LogEntry.Names.Level, LogLevel.Information))
#if DEBUG
                    .UseEcho(
                        new NLogRx(),
                        new ConsoleRx
                        {
                            Renderer = new SimpleConsoleRenderer
                            {
                                Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level:u}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                            }
                        });
#else
                    .UseEcho(new NLogRx());
#endif
            });

            // Add framework services.
            services
                .AddMvc()
                .AddJsonOptions(options => { options.SerializerSettings.Converters.Add(new JsonStringConverter()); });

            services
                .AddScoped<IFeatureToggle>(_ => new FeatureToggle(FeaturePolicy.AlwaysOff));

            services.Configure<RazorViewEngineOptions>(options => { options.ViewLocationExpanders.Add(new RelativeViewLocationExpander("src")); });

            services.AddSingleton(_hostingEnvironment.ContentRootFileProvider);

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


            app.UseOmniLog();

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