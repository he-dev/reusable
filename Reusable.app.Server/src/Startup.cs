using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reusable.OmniLog;
using Reusable.OmniLog.Utilities.AspNetCore;
using Reusable.Utilities.AspNetCore;
using Reusable.Utilities.AspNetCore.Hosting;
using Reusable.Utilities.NLog.LayoutRenderers;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Connectors;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services.Properties;
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

            services.AddWiretap
            (
                LoggerPipelines
                    .Complete
                    .Configure<InvokePropertyService>(node =>
                    {
                        node.Services.Add(new Constant("Environment", _hostingEnvironment.EnvironmentName));
                        node.Services.Add(new Constant("Product", "Reusable.app.Server"));
                    })
                    .Configure<RenameProperty>(node =>
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
                        node.Connectors.Add(new NLogConnector());
                    })
            );

            // Add framework services.
            services
                .AddMvc();
                //.AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringConverter()); });

            services
                .AddScoped<IFeatureToggle>(_ => new FeatureToggle(FeaturePolicy.AlwaysOff));

            services
                .AddScoped<IFeatureController>(s =>
                {
                    var agent = new FeatureController(s.GetService<IFeatureToggle>());
                    return new FeatureTelemetry(agent, s.GetService<ILogger<FeatureTelemetry>>());
                });

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