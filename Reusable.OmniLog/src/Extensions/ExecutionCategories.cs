using System;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Extensions
{
    [PublicAPI]
    public static class ExecutionCategories
    {
        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Property(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Argument(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);

        public static Action<ILogEntry> Flow(this Action<ILogEntry> node, string decision, string? because = default)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Default.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Push(new LogProperty(Names.Default.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Push(new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                if (because is {})
                {
                    e.Push(new LogProperty(Names.Default.Message, because, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                }
            });
        }

        public static Action<ILogEntry> Step(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> WorkItem(this Action<ILogEntry> node, string name, object value) => node.Telemetry().Category(name, value);
        
        private static Action<ILogEntry> Category(this Action<ILogEntry> node, string snapshotName, object snapshot, [CallerMemberName] string? name = null)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(nameof(Category), name!, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Push(new LogProperty(Names.Default.SnapshotName, snapshotName, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Push(new LogProperty(Names.Default.Snapshot, snapshot, LogPropertyMeta.Builder.ProcessWith<DestructureNode>()));
            });
        }

        #region For backward compatibility

        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Default.Category, nameof(Variable), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Default.Category, nameof(Counter), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Default.Category, nameof(Meta), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Flow(this Action<ILogEntry> node)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Default.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }

        public static Action<ILogEntry> Decision(this Action<ILogEntry> node, string decision)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Default.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
                e.Push(new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>()));
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
                    .Push(Names.Default.SnapshotName, dictionary.First().Key, m => m.ProcessWith<EchoNode>())
                    .Push(Names.Default.Snapshot, dictionary.First().Value, m => m.ProcessWith<SerializerNode>());
        }

        #endregion
    }
}