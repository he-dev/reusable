using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Middleware
{
    [PublicAPI]
    [UsedImplicitly]
    public class ResourceTelemetry : ResourceMiddleware
    {
        private readonly ILogger _logger;

        public ResourceTelemetry(RequestDelegate<ResourceContext> next, ILogger<ResourceTelemetry> logger) : base(next)
        {
            _logger = logger;
        }

        public Action<ILogger, ResourceContext> LogRequest { get; set; } = ResourceTelemetryHelper.LogRequest;

        public Action<ILogger, ResourceContext> LogResponse { get; set; } = ResourceTelemetryHelper.LogResponse;

        public Func<ResourceContext, bool> Filter { get; set; } = _ => true;

        public override async Task InvokeAsync(ResourceContext context)
        {
            if (Filter(context))
            {
                await LogTelemetry(context);
            }
            else
            {
                await Next(context);
            }
        }

        private async Task LogTelemetry(ResourceContext context)
        {
            using (_logger.BeginScope().WithCorrelationHandle("ResourceTelemetry").UseStopwatch())
            {
                try
                {
                    LogRequest(_logger, context);

                    await Next(context);

                    LogResponse(_logger, context);
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create
                    (
                        "ResourceRequest",
                        $"Could not {context.Request.Method.ToString().ToUpper()} '{context.Request.ResourceName}'. See the inner exception for details.",
                        inner
                    );
                }
            }
        }
    }

    public static class ResourceTelemetryHelper
    {
        public static void LogRequest(ILogger logger, ResourceContext context)
        {
            logger.Log(Abstraction.Layer.IO().Meta(new
            {
                resourceRequest = new
                {
                    method = context.Request.Method.ToString(),
                    resourceName = context.Request.ResourceName,
                    controllerName = context.Request.ControllerName.ToString(),
                    items = context.Request.Items,
                }
            }));
        }

        public static void LogResponse(ILogger logger, ResourceContext context)
        {
            logger.Log(Abstraction.Layer.IO().Meta(new
            {
                resourceResponse = new
                {
                    statusCode = context.Response.StatusCode,
                    cached = context.Response.Cached,
                    bodyType = context.Response.Body?.GetType().ToPrettyString()
                }
            }));
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