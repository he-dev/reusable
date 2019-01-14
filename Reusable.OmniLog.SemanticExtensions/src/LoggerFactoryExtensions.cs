using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions.Attachments;
using Reusable.Utilities.JsonNet;

namespace Reusable.OmniLog.SemanticExtensions
{
    public static class LoggerFactoryExtensions
    {
        public static LoggerFactory AttachScope(this LoggerFactory loggerFactory, Action<JsonSerializerSettings> configureScope = null)
        {
            var scopeSerializer = new JsonSerializer();
            configureScope?.Invoke(scopeSerializer.Settings);
            return loggerFactory.Attach(new Scope(scopeSerializer));
        }

        public static LoggerFactory AttachSnapshot(this LoggerFactory loggerFactory, Action<JsonSerializerSettings> configureSnapshot = null)
        {
            var snapshotSerializer = new JsonSerializer();
            configureSnapshot?.Invoke(snapshotSerializer.Settings);
            return loggerFactory.Attach(new Snapshot(snapshotSerializer));
        }

        public static LoggerFactory AttachElapsedMilliseconds(this LoggerFactory loggerFactory, string name = "Elapsed")
        {
            return loggerFactory.Attach(new ElapsedMilliseconds(name));
        }
    }
}
