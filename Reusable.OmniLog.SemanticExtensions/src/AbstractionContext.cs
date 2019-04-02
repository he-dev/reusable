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
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.SemanticExtensions.Attachments;
using Reusable.Reflection;

namespace Reusable.OmniLog.SemanticExtensions
{
    public interface IAbstractionContext : IAbstractionLayer, IAbstractionCategory
    {
        /// <summary>
        /// Logs this context. This method supports the Framework infrastructure and is not intended to be used directly in your code.
        /// </summary>
        void Log(ILogger logger, Action<Log> configureLog);
    }

    public static class AbstractionProperties
    {
        public const string Layer = nameof(Layer);
        public const string LogLevel = nameof(LogLevel);
        public const string Category = nameof(Category);
        public const string Identifier = nameof(Identifier);
        public const string Snapshot = nameof(Snapshot);
        public const string Message = nameof(Message);
    }

    [PublicAPI]
    public readonly struct AbstractionContext : IAbstractionContext
    {
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

        private readonly IImmutableDictionary<string, object> _values;

        public AbstractionContext(IImmutableDictionary<string, object> values)
        {
            _values = values;
        }

        public AbstractionContext(IImmutableDictionary<string, object> values, string property, [CallerMemberName] string name = null)
            : this(values.Add(property, name)) { }

        public AbstractionContext(string property, [CallerMemberName] string name = null)
            : this(ImmutableDictionary<string, object>.Empty.Add(property, name)) { }

        public static IAbstractionContext Empty => new AbstractionContext();

        private LogLevel LogLevel
        {
            get
            {
                if (_values.TryGetValue(AbstractionProperties.LogLevel, out var logLevel)) return (LogLevel)logLevel;
                if (CategoryLogLevel.TryGetValue((string)_values[AbstractionProperties.Category], out var logLevelByCategory)) return logLevelByCategory;
                if (LayerLogLevel.TryGetValue((string)_values[AbstractionProperties.Layer], out var logLevelByLayer)) return logLevelByLayer;
                throw DynamicException.Create
                (
                    "LogLevelNotFound",
                    $"Neither category '{_values[AbstractionProperties.Category]}' nor layer '{_values[AbstractionProperties.Layer]}' map to a valid log-level."
                );
            }
        }

        public IAbstractionContext SetItem(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new AbstractionContext((_values ?? ImmutableDictionary<string, object>.Empty).Add(name, value));
        }

        public void Log(ILogger logger, Action<Log> configureLog)
        {
            // In some cases Snapshot can already have an Identifier. If this is the case then use this one instead of enumerating its properties.
            var snapshotProperties =
                _values.TryGetValue(AbstractionProperties.Snapshot, out var snapshot)
                    ? _values.TryGetValue(AbstractionProperties.Identifier, out var identifier)
                        ? new[] { new KeyValuePair<string, object>((string)identifier, snapshot) }
                        : snapshot.EnumerateProperties()
                    : Enumerable.Empty<KeyValuePair<string, object>>();

            var values = _values;

            foreach (var snapshotItem in snapshotProperties)
            {
                logger.Log(LogLevel, log =>
                {
                    log.With(AbstractionProperties.Layer, values[AbstractionProperties.Layer]);
                    log.With(AbstractionProperties.Category, values[AbstractionProperties.Category]);
                    log.With(AbstractionProperties.Identifier, snapshotItem.Key);
                    log.With(Snapshot.BagKey, snapshotItem.Value);
                    if (values.TryGetValue(AbstractionProperties.Message, out var message))
                    {
                        log.Message(message);
                    }

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

        private static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type of a dictionary");
        }
    }
}