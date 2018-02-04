using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.DateTimes;
using Reusable.OmniLog.Collections;
using Reusable.Utilities.ThirdParty.JsonNet;

namespace Reusable.OmniLog.SemanticExtensions
{
    /// <summary>
    /// This helper delegate is meant to be used for dependency injection.
    /// </summary>
    /// <returns></returns>
    public delegate ILoggerFactory SetupLoggerFactoryFunc([NotNull] string environment, [NotNull] string product, [NotNull] Func<IObserver<Log>> createRx);
    //public delegate ILoggerFactory SetupLoggerFactoryFunc([NotNull] string environment, [NotNull] string product, [NotNull] Func<IEnumerable<ILogScopeMerge>, IObserver<Log>> createRx);

    /// <summary>
    /// This class provides methods that create ILoggerFactory that is already set-up for semantic logging.
    /// </summary>
    public class LoggerFactorySetup
    {
        [NotNull]
        public static ILoggerFactory SetupLoggerFactory([NotNull] string environment, [NotNull] string product, [NotNull] Func<IObserver<Log>> createRx)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (createRx == null) throw new ArgumentNullException(nameof(createRx));

            return new LoggerFactory
            {
                Observers =
                {
                    createRx() //new[] { new LogTransactionMerge() })
                },
                Configuration = new LoggerConfiguration
                {
                    Attachements =
                    {
                        new OmniLog.Attachements.Lambda("Environment", _ => environment),
                        new OmniLog.Attachements.Lambda("Product", _ => product),
                        new OmniLog.Attachements.Timestamp<UtcDateTime>(),
                        new OmniLog.Attachements.Scope(new JsonStateSerializer
                        {
                            Settings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Include,
                                Formatting = Formatting.Indented,
                                Converters =
                                {
                                    new StringEnumConverter(),
                                    new SoftStringConverter(),
                                }
                            }
                        }),
                        new OmniLog.Attachements.ElapsedMilliseconds("Elapsed"),
                        new OmniLog.SemanticExtensions.Attachements.Snapshot()
                    }
                }
            };
        }
    }
}
