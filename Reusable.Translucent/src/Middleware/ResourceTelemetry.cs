using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ResourceTelemetry : MiddlewareBase
    {
        private readonly ILogger _logger;
        private readonly IResourceTelemetryFilter _filter;

        public ResourceTelemetry(RequestDelegate<ResourceContext> next, ILogger<ResourceTelemetry> logger, IResourceTelemetryFilter filter) : base(next)
        {
            _logger = logger;
            _filter = filter;
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            if (_filter.CanLog(context))
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
            using (_logger.BeginScope().WithCorrelationHandle("ResourceTelemetry").UseStopwatch())
            {
                var requestUri = context.Request.ResourceName;
                try
                {
                    _logger.Log(Abstraction.Layer.IO().Meta(new
                    {
                        resourceRequest = new
                        {
                            method = context.Request.Method.ToString(),
                            resourceName = context.Request.ResourceName,
                            controllerName = context.Request.ControllerName.ToString(),
                            items = context.Request.Items,
                        }
                    }));

                    await InvokeNext(context);

                    _logger.Log(Abstraction.Layer.IO().Meta(new
                    {
                        resourceResponse = new
                        {
                            statusCode = context.Response.StatusCode,
                            cached = context.Response.Cached,
                            bodyType = context.Response.Body?.GetType().ToPrettyString()
                        }
                    }));
                }
                catch (Exception inner)
                {
                    _logger.Log(Abstraction.Layer.IO().Routine("ResourceRequest").Faulted(inner), l => l.Message(requestUri));
                    throw;
                }
            }
        }
    }

    public interface IResourceTelemetryFilter
    {
        bool CanLog(ResourceContext context);
    }

    // public static class TelemetryMiddlewareHelper
    // {
    //     public static IPipelineBuilder<TContext> UseTelemetry<TContext>(this IPipelineBuilder<TContext> builder, ILogger<TelemetryMiddleware> logger)
    //     {
    //         return builder.UseMiddleware<TelemetryMiddleware>(logger);
    //     }
    // }
}