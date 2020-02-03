using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Extensions;
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
        private readonly IEnumerable<IResourceTelemetryFilter>? _filters;

        public ResourceTelemetry(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services)
        {
            _logger = services.GetService<ILoggerFactory>().CreateLogger<ResourceTelemetry>();
            _filters = services.GetServices<IResourceTelemetryFilter>();
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            if (_filters is null || _filters.Any(f => f.CanLog(context)))
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
                var requestUri = context.Request.Uri.ToString();
                try
                {
                    _logger.Log(Abstraction.Layer.IO().Meta(new
                    {
                        resourceRequest = new
                        {
                            path = requestUri,
                            method = context.Request.Method.ToString(),
                            controllerName = context.Request.ControllerName.ToString(),
                            maxAge = context.Request.MaxAge,
                            bodyType = context.Request.Body?.GetType().ToPrettyString(),
                            required = context.Request.Required,
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