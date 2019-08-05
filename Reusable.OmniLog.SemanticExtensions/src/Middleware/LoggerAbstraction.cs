using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.SemanticExtensions.Middleware
{
    public class LoggerAbstraction : LoggerMiddleware
    {
        //public static readonly string LogItemTag = nameof(AbstractionBuilder<object>);

        public static readonly string LogPropertyName = nameof(AbstractionBuilder<object>);

        public LoggerAbstraction() : base(true) { }

        public IDictionary<string, LogLevel> LayerLevel { get; set; } = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionLayers.Business)] = LogLevel.Information,
            [nameof(AbstractionLayers.Service)] = LogLevel.Debug,
            [nameof(AbstractionLayers.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionLayers.IO)] = LogLevel.Trace,
            [nameof(AbstractionLayers.Database)] = LogLevel.Trace,
            [nameof(AbstractionLayers.Network)] = LogLevel.Trace,
        };

        protected override void InvokeCore(Log request)
        {
            // Do we have an abstraction-context?
            if (request.TryGetItem<IAbstractionBuilder>((LogPropertyName, Log.ItemTags.Metadata), out var builder))
            {
                var log = builder.Build();

                // Copy all properties.
                foreach (var (key, value) in log.Where(x => x.Key.Tag.Equals(Log.DefaultItemTag)))
                {
                    request.SetItem((key.Name.ToString(), key.Tag.ToString()), value);
                }

                // Use other level only if it's not already set.
                if (!request.TryGetItem<LogLevel>((Log.PropertyNames.Level, default), out _))
                {
                    // Use layer-level as fallback.
                    if (LayerLevel.TryGetValue(log["Layer"].ToString(), out var layerLevel))
                    {
                        request.Level(layerLevel);
                    }
                }
            }

            Next?.Invoke(request);
        }
    }
}