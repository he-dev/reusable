using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Reusable.Collections;
using Reusable.OmniLog.Collections;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Serialization;
using Reusable.Validation;

namespace Reusable.OmniLog.SemanticExtensions
{
    /// <summary>
    /// This helper delegate is meant to be used for dependency injection.
    /// </summary>
    /// <returns></returns>
    public delegate ILoggerFactory SetupLoggerFactoryFunc([NotNull] string environment, [NotNull] string product, [NotNull] IEnumerable<ILogRx> rxs, [CanBeNull] ISerializer serializer = null);

    /// <summary>
    /// This class provides methods that create ILoggerFactory that is already set-up for semantic logging.
    /// </summary>
    public class LoggerFactorySetup
    {
        [Obsolete("Use the new LoggerFactoryBuilder instead.")]
        [NotNull]
        public static ILoggerFactory SetupLoggerFactory([NotNull] string environment, [NotNull] string product, [NotNull] IEnumerable<ILogRx> rxs, [CanBeNull] ISerializer serializer = null)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (rxs == null) throw new ArgumentNullException(nameof(rxs));

            return new LoggerFactory
            {
                Observers = rxs.ToList(),
                Configuration = new LoggerConfiguration
                {
                    Attachements =
                    {
                        new OmniLog.Attachements.Lambda("Environment", _ => environment),
                        new OmniLog.Attachements.Lambda("Product", _ => product),
                        new OmniLog.Attachements.Timestamp<DateTimeUtc>(),
                        new OmniLog.Attachements.Scope(serializer ?? new JsonSerializer
                        {
                            Settings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Include,
                                Formatting = Formatting.Indented,
                                Converters =
                                {
                                    new StringEnumConverter(),
                                    new SoftStringConverter(),
                                    new KeyValuePairConverter<SoftString, object>()
                                },
                                //ContractResolver = new CompositeContractResolver
                                //{
                                //    new InterfaceContractResolver<ILogScope>(),
                                //    new DefaultContractResolver()
                                //}
                            }
                        }),
                        new OmniLog.Attachements.ElapsedMilliseconds("Elapsed"),
                        new OmniLog.SemanticExtensions.Attachements.Snapshot()
                    }
                }
            };
        }
    }

    public class LoggerFactoryBuilder
    {
        private static readonly IDuckValidator<LoggerFactoryBuilder> Validator = new DuckValidator<LoggerFactoryBuilder>(loggerFactory =>
        {
            loggerFactory
                .IsValidWhen(builder => builder._rxes.Any(), "You need to add at least one Rx.")
                .IsNotValidWhen(builder => string.IsNullOrEmpty(builder._environment), "You need to specify the environment.")
                .IsNotValidWhen(builder => string.IsNullOrEmpty(builder._product), "You need to specif the product.");
        });

        private readonly List<ILogRx> _rxes;
        private string _environment;
        private string _product;
        private readonly JsonSerializerSettings _scopeSerializerSettings;
        private readonly JsonSerializerSettings _snapshotSerializerSettings;

        public LoggerFactoryBuilder()
        {
            _rxes = new List<ILogRx>();
            _scopeSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters =
                {
                    new StringEnumConverter(),
                    new SoftStringConverter(),
                    new KeyValuePairConverter<SoftString, object>()
                },
                //ContractResolver = new CompositeContractResolver
                //{
                //    new InterfaceContractResolver<ILogScope>(),
                //    new DefaultContractResolver()
                //}
            };

            _snapshotSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters =
                {
                    new StringEnumConverter(),
                    new SoftStringConverter(),
                }
            };
        }

        public LoggerFactoryBuilder WithRx([NotNull] ILogRx rx)
        {
            _rxes.Add(rx ?? throw new ArgumentNullException(nameof(rx)));
            return this;
        }

        public LoggerFactoryBuilder WithRxes([NotNull] IEnumerable<ILogRx> rxes)
        {
            _rxes.AddRange(rxes ?? throw new ArgumentNullException(nameof(rxes)));
            return this;
        }

        public LoggerFactoryBuilder Environment(string environment)
        {
            _environment = environment;
            return this;
        }

        public LoggerFactoryBuilder Product(string product)
        {
            _product = product;
            return this;
        }

        public LoggerFactoryBuilder ScopeSerializer(Action<JsonSerializerSettings> serializer)
        {
            serializer(_scopeSerializerSettings);
            return this;
        }

        public LoggerFactoryBuilder SnapshotSerializer(Action<JsonSerializerSettings> serializer)
        {
            serializer(_snapshotSerializerSettings);
            return this;
        }

        public ILoggerFactory Build()
        {
            Validator.Validate(this);

            return new LoggerFactory
            {
                Observers = _rxes,
                Configuration = new LoggerConfiguration
                {
                    Attachements =
                    {
                        new OmniLog.Attachements.Lambda("Environment", _ => _environment),
                        new OmniLog.Attachements.Lambda("Product", _ => _product),
                        new OmniLog.Attachements.Timestamp<DateTimeUtc>(),
                        new OmniLog.Attachements.Scope(new JsonSerializer{ Settings = _scopeSerializerSettings }),
                        new OmniLog.Attachements.ElapsedMilliseconds("Elapsed"),
                        new OmniLog.SemanticExtensions.Attachements.Snapshot(new JsonSerializer{ Settings = _snapshotSerializerSettings })
                    }
                }
            };
        }
    }
}
