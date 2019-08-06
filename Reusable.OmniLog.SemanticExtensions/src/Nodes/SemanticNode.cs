using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.SemanticExtensions.Nodes
{
    public class SemanticNode : LoggerNode
    {
        public static readonly string LogPropertyName = nameof(Abstraction);

        public SemanticNode() : base(true) { }

        //public override bool IsActive => base.IsActive && !(Next is null);

        public IDictionary<string, LogLevel> LayerLevel { get; set; } = new Dictionary<string, LogLevel>(SoftString.Comparer)
        {
            [nameof(AbstractionLayers.Business)] = LogLevel.Information,
            [nameof(AbstractionLayers.Service)] = LogLevel.Debug,
            [nameof(AbstractionLayers.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionLayers.IO)] = LogLevel.Trace,
            [nameof(AbstractionLayers.Database)] = LogLevel.Trace,
            [nameof(AbstractionLayers.Network)] = LogLevel.Trace,
        };

        protected override void InvokeCore(LogEntry request)
        {
            // Do we have an abstraction-builder?
            if (request.TryGetItem<IAbstractionBuilder>(LogPropertyName, LogEntry.ItemTags.Metadata, out var builder))
            {
                var logEntry = builder.Build();

                // Copy all items to the main log.
                foreach (var item in logEntry)
                {
                    request.SetItem(item.Key.Name, item.Key.Tag, item.Value);
                }

                // Use other level only if it's not already set.
                if (!request.TryGetItem<LogLevel>(LogEntry.BasicPropertyNames.Level, default, out _))
                {
                    // Use layer-level as fallback.
                    if (LayerLevel.TryGetValue(logEntry["Layer"].ToString(), out var layerLevel))
                    {
                        request.Level(layerLevel);
                    }
                }
            }

            Next?.Invoke(request);
        }
    }
}