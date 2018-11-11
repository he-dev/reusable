using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.SemanticExtensions.Attachements;

namespace Reusable.OmniLog.SemanticExtensions
{
    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static LoggerFactory UseSemanticExtensions
        (
            this LoggerFactory loggerFactory,
            string environment,
            string product,
            Action<JsonSerializerSettings> configureScope = null,
            Action<JsonSerializerSettings> configureSnapshot = null
        )
        {
            var scopeSerializer = new JsonSerializer();
            var snapshotSerializer = new JsonSerializer();

            configureScope?.Invoke(scopeSerializer.Settings);
            configureSnapshot?.Invoke(snapshotSerializer.Settings);

            return
                loggerFactory
                    .Attach("Environment", _ => environment)
                    .Attach("Product", _ => product)
                    .Attach<Timestamp<DateTimeUtc>>()
                    .Attach(new Scope(scopeSerializer))
                    .Attach(new ElapsedMilliseconds("Elapsed"))
                    .Attach(new Snapshot(snapshotSerializer));
        }
    }
}
