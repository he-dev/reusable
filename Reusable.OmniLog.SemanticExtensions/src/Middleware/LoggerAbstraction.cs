using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.SemanticExtensions.Middleware
{
    using acpn = AbstractionContext.PropertyNames;

    public class LoggerAbstraction : LoggerMiddleware
    {
        public static readonly string LogItemTag = nameof(AbstractionContext);
        
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

        protected override void InvokeCore(Log request)
        {
            // Do we have an abstraction-context?
            if (request.TryGetItem<IAbstractionContext>((Log.PropertyNames.Metadata, LogItemTag), out var context))
            {
                // Use other level only if it's not already set.
                if (!request.TryGetItem<LogLevel>((acpn.Level, default), out _))
                {
                    // Context's level overrides layer level.
                    if (context.Values.TryGetValue(acpn.Level, out var contextLevel))
                    {
                        request.SetItem((acpn.Level, default), contextLevel);
                    }
                    else
                    {
                        // Use layer-level as fallback.
                        if (LayerLevel.TryGetValue(context.Values[acpn.Layer].ToString(), out var layerLevel))
                        {
                            request.SetItem((acpn.Level, default), layerLevel);
                        }
                    }
                }

                request.SetItem((acpn.Layer, default), context.Values[acpn.Layer]);
                request.SetItem((acpn.Category, default), context.Values[acpn.Category]);

                if (context.Values.TryGetValue(acpn.Because, out var because) && because is string message)
                {
                    request.Message(message);
                }
            }

            Next?.Invoke(request);
        }
    }
}