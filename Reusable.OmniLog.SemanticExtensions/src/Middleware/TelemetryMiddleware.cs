using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
//using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class TelemetryMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;
        private readonly ILogger<TelemetryMiddleware> _logger;

        public TelemetryMiddleware(RequestDelegate<ResourceContext> next, ILogger<TelemetryMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            using (_logger.UseScope().WithCorrelationHandle("CollectMiddlewareTelemetry").UseStopwatch())
            {
                var requestUri = context.Request.Uri.ToString();
                try
                {
                    await _next(context);
                    _logger.Log(Abstraction.Layer.IO().Meta(new
                    {
                        requestUri,
                        statusCode = context.Response.StatusCode,
                        isOptional = context.Request.Metadata.GetItemOrDefault(Request.IsOptional),
                        //exists = context.Response?.Exists()
                    }, "Resource"));
                }
                catch (Exception inner)
                {
                    _logger.Log(Abstraction.Layer.IO().Routine("ResourceRequest").Faulted(inner), l => l.Message(requestUri));
                    throw;
                }
            }
        }
    }

    public static class TelemetryMiddlewareHelper
    {
        public static IResourceRepositoryBuilder<TContext> UseTelemetry<TContext>(this IResourceRepositoryBuilder<TContext> builder, ILogger<TelemetryMiddleware> logger)
        {
            return builder.UseMiddleware<TelemetryMiddleware>(logger);
        }
    }
}