using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceTelemetry : MiddlewareBase
    {
        private readonly ILogger _logger;

        public ResourceTelemetry(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services)
        {
            _logger = services.GetService<ILoggerFactory>().CreateLogger<ResourceTelemetry>();
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            using (_logger.BeginScope().WithCorrelationHandle("ResourceTelemetry").UseStopwatch())
            {
                var requestUri = context.Request.Uri.ToString();
                try
                {
                    _logger.Log(Abstraction.Layer.IO().Meta(new
                    {
                        resource = new
                        {
                            path = requestUri,
                            required = context.Request.Required,
                        }
                    }));
                    await InvokeNext(context);
                }
                catch (Exception inner)
                {
                    _logger.Log(Abstraction.Layer.IO().Routine("ResourceRequest").Faulted(inner), l => l.Message(requestUri));
                    throw;
                }
            }
        }
    }

    // public static class TelemetryMiddlewareHelper
    // {
    //     public static IPipelineBuilder<TContext> UseTelemetry<TContext>(this IPipelineBuilder<TContext> builder, ILogger<TelemetryMiddleware> logger)
    //     {
    //         return builder.UseMiddleware<TelemetryMiddleware>(logger);
    //     }
    // }
}