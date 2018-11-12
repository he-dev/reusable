using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog.SemanticExtensions.Attachements;

namespace Reusable.OmniLog.SemanticExtensions
{
    public interface IAbstractionContext
    {
        /// <summary>
        /// Logs this context. This method supports the Framework infrastructure and is not intended to be used directly in your code.
        /// </summary>
        void Log(ILogger logger, Action<Log> configureLog);
    }

    public class AbstractionContext : IAbstractionContext
    {
        public static IDictionary<SoftString, LogLevel> LogLevels = new Dictionary<SoftString, LogLevel>
        {
            [nameof(AbstractionLayerExtensions.Meta)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Monitoring)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Infrastructure)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.IO)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Database)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Network)] = LogLevel.Trace,
        };

        public AbstractionContext(IAbstractionLayer layer, object dump, [CallerMemberName] string category = null)
        {
            (Layer, LogLevel) = layer;
            Category = category;
            Dump = dump;
        }

        public LogLevel LogLevel { get; }

        public string Layer { get; }

        public string Category { get; }

        public object Dump { get; }
        
        public virtual void Log(ILogger logger, Action<Log> configureLog)
        {
            var properties =
                Dump is null
                    ? Enumerable.Empty<KeyValuePair<string, object>>()
                    : Dump.EnumerateProperties();

            foreach (var dump in properties)
            {
                logger.Log(LogLevel, log =>
                {
                    log.With("Identifier", dump.Key);
                    log.With(nameof(Category), Category);
                    log.With(nameof(Layer), Layer);
                    log.With(Snapshot.BagKey, dump.Value);
                    configureLog?.Invoke(log);
                });
            }
        }
    }
}
