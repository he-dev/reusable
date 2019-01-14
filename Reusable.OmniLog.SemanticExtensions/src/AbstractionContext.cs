using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.OmniLog.SemanticExtensions.Attachments;
using Reusable.Reflection;

namespace Reusable.OmniLog.SemanticExtensions
{
    public interface IAbstractionContext
    {
        IImmutableDictionary<string, object> Values { get; }

        /// <summary>
        /// Logs this context. This method supports the Framework infrastructure and is not intended to be used directly in your code.
        /// </summary>
        void Log(ILogger logger, Action<Log> configureLog);
    }

    [PublicAPI]
    public readonly struct AbstractionContext : IAbstractionLayer, IAbstractionCategory, IAbstractionContext
    {
        public static class PropertyNames
        {
            public const string Layer = nameof(Layer);
            public const string Category = nameof(Category);
            public const string Identifier = nameof(Identifier);
            public const string Snapshot = nameof(Snapshot);
            public const string LogLevel = nameof(LogLevel);
        }

        public static IDictionary<string, LogLevel> LayerLogLevel = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionExtensions.Business)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Infrastructure)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.IO)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Database)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Network)] = LogLevel.Trace,
        };

        public static IDictionary<string, LogLevel> CategoryLogLevel = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionLayerExtensions.Counter)] = LogLevel.Information,
            [nameof(AbstractionLayerExtensions.Meta)] = LogLevel.Debug,
            [nameof(AbstractionLayerExtensions.Variable)] = LogLevel.Trace,
            [nameof(AbstractionLayerExtensions.Property)] = LogLevel.Trace,
            [nameof(AbstractionLayerExtensions.Argument)] = LogLevel.Trace,
        };

        public AbstractionContext(IImmutableDictionary<string, object> values)
        {
            Values = values;
        }

        public AbstractionContext(IImmutableDictionary<string, object> values, string property, [CallerMemberName] string name = null)
            : this(values.Add(property, name))
        { }

        public AbstractionContext(string property, [CallerMemberName] string name = null)
            : this(ImmutableDictionary<string, object>.Empty.Add(property, name))
        { }


        public IImmutableDictionary<string, object> Values { get; }

        private LogLevel LogLevel
        {
            get
            {
                if (Values.TryGetValue(PropertyNames.LogLevel, out var logLevel)) return (LogLevel)logLevel;
                if (CategoryLogLevel.TryGetValue((string)Values[PropertyNames.Category], out var logLevelByCategory)) return logLevelByCategory;
                if (LayerLogLevel.TryGetValue((string)Values[PropertyNames.Layer], out var logLevelByLayer)) return logLevelByLayer;
                throw DynamicException.Create
                (
                    "LogLevelNotFound",
                    $"Neither category '{Values[PropertyNames.Category]}' nor layer '{Values[PropertyNames.Layer]}' map to a valid log-level."
                );
            }
        }

        public void Log(ILogger logger, Action<Log> configureLog)
        {
            // In some cases Snapshot can already have an Identifier. If this is then case then use this one instead of enumerating its properties.
            var properties =
                Values.TryGetValue(PropertyNames.Snapshot, out var snapshot)
                    ? Values.TryGetValue(PropertyNames.Identifier, out var identifier)
                        ? new[] { new KeyValuePair<string, object>((string)identifier, snapshot) }
                        : snapshot.EnumerateProperties()
                    : Enumerable.Empty<KeyValuePair<string, object>>();

            var values = Values;

            foreach (var dump in properties)
            {
                logger.Log(LogLevel, log =>
                {
                    // todo - this could be a loop over 'values'
                    log.With(PropertyNames.Identifier, dump.Key);
                    log.With(PropertyNames.Category, values[PropertyNames.Category]);
                    log.With(PropertyNames.Layer, values[PropertyNames.Layer]);
                    log.With(Snapshot.BagKey, dump.Value);
                    configureLog?.Invoke(log);
                });
            }
        }
    }

    public static class ObjectExtensions
    {
        public static IEnumerable<KeyValuePair<string, object>> EnumerateProperties<T>(this T obj)
        {
            return
                obj is IDictionary<string, object> dictionary
                    ? dictionary
                    : obj
                        .GetType()
                        .ValidateIsAnonymous()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property => new KeyValuePair<string, object>(property.Name, property.GetValue(obj)));
        }

        public static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type of a dictionary");
        }
    }
}
