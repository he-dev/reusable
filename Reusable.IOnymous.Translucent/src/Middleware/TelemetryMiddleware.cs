using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

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
            var stopwatch = _logger.UseStopwatch();
            try
            {
                await _next(context);
                _logger.Log(Abstraction.Layer.IO().Routine("ResourceRequest").Completed(), l => l.Message(context.Request.Uri.ToString()));
            }
            catch (Exception inner)
            {
                _logger.Log(Abstraction.Layer.IO().Routine("ResourceRequest").Faulted(inner), l => l.Message(context.Request.Uri.ToString()));
                throw;
            }
            finally
            {
                stopwatch.Dispose();
            }
        }
    }
}