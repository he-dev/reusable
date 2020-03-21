using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog {}

namespace Reusable.OmniLog.SemanticExtensions
{
    public abstract class Names : Reusable.OmniLog.Names
    {
        public abstract class Custom
        {
            public const string Layer = nameof(Layer);
            public const string Category = nameof(Category);
        }
    }

    public abstract class Application
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        [Obsolete("Use 'Context'.")]
        public static readonly Action<ILogEntry> Layer = _ => { };

        public static readonly Action<ILogEntry> Context = _ => { };
    }

    [PublicAPI]
    public static class ApplicationLayers
    {
        public static Action<ILogEntry> Service(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Telemetry(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> IO(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Database(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Network(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Business(this Action<ILogEntry> node) => node.Layer();
        public static Action<ILogEntry> Presentation(this Action<ILogEntry> node) => node.Layer();
    }

    [PublicAPI]
    public static class ApplicationLayerCategories
    {
        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Property(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Argument(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);

        public static Action<ILogEntry> Flow(this Action<ILogEntry> node, string decision, string? because = default)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(Names.Custom.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Add(new LogProperty(Names.Default.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Add(new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                if (because is {})
                {
                    e.Add(new LogProperty(Names.Default.Message, because, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                }
            });
        }

        public static Action<ILogEntry> Step(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> WorkItem(this Action<ILogEntry> node, string name, object value) => node.Telemetry().Category(name, value);

        #region For backward compatibility

        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(Names.Custom.Category, nameof(Variable), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(Names.Custom.Category, nameof(Counter), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(Names.Custom.Category, nameof(Meta), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Flow(this Action<ILogEntry> node)
        {
            return node.Then(e => e.Add(new LogProperty(Names.Custom.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }

        public static Action<ILogEntry> Decision(this Action<ILogEntry> node, string decision)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(Names.Default.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Add(new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
            });
        }

        public static Action<ILogEntry> Because(this Action<ILogEntry> node, string because)
        {
            return node.Then(e => { e.Message(because); });
        }

        private static ILogEntry Snapshot(this ILogEntry logEntry, object snapshot)
        {
            var dictionary = snapshot.ToDictionary();
            return
                logEntry
                    .Add(Names.Default.SnapshotName, dictionary.First().Key, m => m.ProcessWith<EchoNode>())
                    .Add(Names.Default.Snapshot, dictionary.First().Value, m => m.ProcessWith<SerializerNode>());
        }

        #endregion
    }

    public static class ApplicationLogLevels
    {
        public static Action<ILogEntry> Trace(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Debug(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Warning(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Information(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Error(this Action<ILogEntry> node) => node.Level();
        public static Action<ILogEntry> Fatal(this Action<ILogEntry> node) => node.Level();
    }

    public static class LogEntryExtensions
    {
        public static Action<ILogEntry> Layer(this Action<ILogEntry> node, [CallerMemberName] string? name = null)
        {
            return node.Then(e => e.Add(new LogProperty(nameof(Layer), name!, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }

        public static Action<ILogEntry> Level(this Action<ILogEntry> node, [CallerMemberName] string? name = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.Level((LogLevel)Enum.Parse(typeof(LogLevel), name));
        }

        public static Action<ILogEntry> Level(this Action<ILogEntry> node, LogLevel logLevel)
        {
            return node.Then(e => e.Add(new LogProperty(Names.Default.Level, logLevel, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }

        public static Action<ILogEntry> Category(this Action<ILogEntry> node, string snapshotName, object snapshot, [CallerMemberName] string? name = null)
        {
            return node.Then(e =>
            {
                e.Add(new LogProperty(nameof(Category), name!, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Add(new LogProperty(Names.Default.SnapshotName, snapshotName, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Add(new LogProperty(Names.Default.Snapshot, snapshot, LogPropertyMeta.Builder.ProcessWith<DestructureNode>()));
            });
        }

        public static Action<ILogEntry> Message(this Action<ILogEntry> node, string message)
        {
            return node.Then(e => e.Add(new LogProperty(Names.Default.Message, message, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }

        public static Action<ILogEntry> Exception(this Action<ILogEntry> node, Exception exception)
        {
            return node.Then(e => e.Add(new LogProperty(Names.Default.Exception, exception, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }
    }

    public static class Decision
    {
        public static IEnumerable<LogProperty> Make(string decision, string? because = default)
        {
            yield return new LogProperty(Names.Custom.Category, nameof(ApplicationLayerCategories.Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.SnapshotName, nameof(Decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            if (because is {})
            {
                yield return new LogProperty(Names.Default.Message, because, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            }
        }
    }
}