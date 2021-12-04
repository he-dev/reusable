using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using LoggerScopeExtensions = Reusable.Wiretap.Extensions.LoggerScopeExtensions;

namespace Reusable.Translucent.Middleware
{
    [PublicAPI]
    [UsedImplicitly]
    public class ResourceTelemetry : ResourceMiddleware
    {
        private readonly ILogger _logger;

        public ResourceTelemetry(ILogger<ResourceTelemetry> logger) => _logger = logger;

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
                await InvokeNext(context);
            }
        }

        private async Task LogTelemetry(ResourceContext context)
        {
            using (_logger.BeginScope("RequestResource"))
            {
                try
                {
                    LogRequest(_logger, context);

                    await InvokeNext(context);

                    context.Response!.Log = context.Log;

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
            logger.Log(Telemetry.Collect.Application().Metadata("ResourceRequest", new
            {
                method = context.Request.Method.ToString().ToUpper(),
                resourceName = context.Request.ResourceName,
                controllerName = context.Request.ControllerName?.ToString(),
                controllerTags = context.Request.ControllerTags,
                items = context.Request.Items,
            }));
        }

        public static void LogResponse(ILogger logger, ResourceContext context)
        {
            logger.Log(Telemetry.Collect.Application().Metadata("ResourceResponse", new
            {
                statusCode = context.Response!.StatusCode,
                bodyType = context.Response!.Body?.GetType().ToPrettyString()
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