using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.v2;

namespace Reusable.OmniLog.SemanticExtensions.v2.Middleware
{
    using Reusable.OmniLog.Abstractions.v2;
    using Reusable.OmniLog.SemanticExtensions.v2;
    using acpn = AbstractionContext.PropertyNames;

    public class LoggerAbstraction : LoggerMiddleware
    {
        public LoggerAbstraction() : base(true) { }

        public IDictionary<string, LogLevel> LayerLevel { get; set; } = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionExtensions.Business)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Service)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.IO)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Database)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Network)] = LogLevel.Trace,
        };

        protected override void InvokeCore(ILog request)
        {
            // Do we have an abstraction-context?
            if (request.TryGetValue(nameof(AbstractionContext), out var obj) && obj is IAbstractionContext context)
            {
                // Use other level only if it's not already set.
                if (!request.ContainsKey(acpn.Level))
                {
                    // Context's level overrides layer level.
                    if (context.Values.TryGetValue(acpn.Level, out var contextLevel))
                    {
                        request.SetItem(acpn.Level, contextLevel);
                    }
                    else
                    {
                        // Use layer-level as fallback.
                        if (LayerLevel.TryGetValue(context.Values[acpn.Layer].ToString(), out var layerLevel))
                        {
                            request.SetItem(acpn.Level, layerLevel);
                        }
                    }
                }

                request.SetItem(acpn.Layer, context.Values[acpn.Layer]);
                request.SetItem(acpn.Category, context.Values[acpn.Category]);

                if (context.Values.TryGetValue(acpn.Because, out var because) && because is string message)
                {
                    request.Message(message);
                }
            }

            Next?.Invoke(request);
        }
    }
}