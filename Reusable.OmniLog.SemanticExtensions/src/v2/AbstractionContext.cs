using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.OmniLog.SemanticExtensions.v2
{
    using Reusable.OmniLog.v2;
    using v1 = Reusable.OmniLog.Abstractions;
    using v2 = Reusable.OmniLog.Abstractions.v2;

    public interface IAbstractionContext : IAbstractionLayer, IAbstractionCategory
    {
        /// <summary>
        /// Logs this context. This method supports the Framework infrastructure and is not intended to be used directly in your code.
        /// </summary>
        void Log(v2.ILogger logger, Action<v1.ILog> transform);
    }

    public static class AbstractionProperties
    {
        public const string Layer = nameof(Layer);
        public const string LogLevel = nameof(LogLevel);

        public const string Category = nameof(Category);

        public const string Identifier = nameof(Identifier);
        public const string Snapshot = nameof(Snapshot);
        public const string Routine = nameof(Routine);
        public const string Message = nameof(Message);
    }

    [PublicAPI]
    public readonly struct AbstractionContext : IAbstractionContext
    {
        public static IDictionary<string, LogLevel> LayerLogLevel = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionExtensions.Business)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Infrastructure)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Service)] = LogLevel.Debug,
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

        public object GetItem(string name)
        {
            return _values[name];
        }

        public IAbstractionContext SetItem(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            return new AbstractionContext((_values ?? ImmutableDictionary<string, object>.Empty).Add(name, value));
        }

        public void Log(v2.ILogger logger, Action<v1.ILog> transform)
        {
            var snapshotProperties =
                _values.TryGetValue(AbstractionProperties.Snapshot, out var snapshot)
                    ? snapshot.EnumerateProperties()
                    : Enumerable.Empty<(string, object)>();
            
            var values = _values; // <-- cannot access _values because of struct
            var level = LogLevel;
            
            foreach (var (name, value) in snapshotProperties)
            {
                logger.Log(log =>
                {
                    log.SetItem(LogPropertyNames.Level, level);
                    log.SetItem(AbstractionProperties.Layer, values[AbstractionProperties.Layer]);
                    log.SetItem(AbstractionProperties.Category, values[AbstractionProperties.Category]);
                    log.SetItem(AbstractionProperties.Identifier, name);
                    log.AttachSerializable(AbstractionProperties.Snapshot, value);
                    if (values.TryGetValue(AbstractionProperties.Message, out var obj) && obj is string message)
                    {
                        log.Message(message);
                    }

                    transform?.Invoke(log);
                });
            }
        }
    }

    public static class ObjectExtensions
    {
        public static IEnumerable<(string Name, object Value)> EnumerateProperties<T>(this T obj)
        {
            return
                obj is IDictionary<string, object> dictionary
                    ? dictionary.Select(item => (item.Key, item.Value))
                    : obj
                        .GetType()
                        .ValidateIsAnonymous()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property => (property.Name, property.GetValue(obj)));
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