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
                e.Push(new LogProperty(Names.Properties.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                if (because is {})
                {
                    e.Push(new LogProperty(Names.Properties.Message, because, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                }
            });
        }

        public static Action<ILogEntry> Step(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> WorkItem(this Action<ILogEntry> node, string name, object value) => node.Telemetry().Category(name, value);
        public static Action<ILogEntry> Routine(this Action<ILogEntry> node, string name) => node.Telemetry().Category(name, default);

        private static Action<ILogEntry> Category(this Action<ILogEntry> node, string snapshotName, object? snapshot, [CallerMemberName] string? name = null)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(nameof(Category), name!, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.SnapshotName, snapshotName, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                if (snapshot is {})
                {
                    e.Push(new LogProperty(Names.Properties.Snapshot, snapshot, LogPropertyMeta.Builder.ProcessWith<Destructure>()));
                }
            });
        }

        #region For backward compatibility

        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Variable), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Counter), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Meta), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        public static Action<ILogEntry> Flow(this Action<ILogEntry> node)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Properties.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static Action<ILogEntry> Decision(this Action<ILogEntry> node, string decision)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.SnapshotName, nameof(decision), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<Echo>()));
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
                    .Push(Names.Properties.SnapshotName, dictionary.First().Key, m => m.ProcessWith<Echo>())
                    .Push(Names.Properties.Snapshot, dictionary.First().Value, m => m.ProcessWith<SerializeProperty>());
        }

        #endregion
    }
}