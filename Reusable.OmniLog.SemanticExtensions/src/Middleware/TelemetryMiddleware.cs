using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
//using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Middleware
{
    [UsedImplicitly]
    public class TelemetryMiddleware
    {
        private readonly RequestCallback<ResourceContext> _next;
        private readonly ILogger<TelemetryMiddleware> _logger;

        public TelemetryMiddleware(RequestCallback<ResourceContext> next, ILogger<TelemetryMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            using (_logger.UseScope(correlationHandle: "ResourceTelemetry"))
            using (_logger.UseStopwatch())
            {
                var requestUri = context.Request.Uri.ToString();
                try
                {
                    await _next(context);
                    var responseUri = context.Response?.Uri?.ToString();
                    _logger.Log(Abstraction.Layer.IO().Meta(new { requestUri, responseUri, exists = context.Response?.Exists }, "Resource"));
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
        public static MiddlewareBuilder UseTelemetry(this MiddlewareBuilder builder, ILogger<TelemetryMiddleware> logger)
        {
            return builder.Add<TelemetryMiddleware>(logger);
        }
    }
}