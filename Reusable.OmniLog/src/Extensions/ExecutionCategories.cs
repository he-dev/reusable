using System;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Extensions
{
    [PublicAPI]
    public static partial class ExecutionCategories
    {
        // Categories

        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Property(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Argument(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Step(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, string name, object value) => node.Category(name, value);
        public static Action<ILogEntry> WorkItem(this Action<ILogEntry> node, string name, object value) => node.Telemetry().Category(name, value);
        public static Action<ILogEntry> Routine(this Action<ILogEntry> node, string name) => node.Telemetry().Category(name, default);

        private static Action<ILogEntry> Category(this Action<ILogEntry> node, string snapshotName, object? snapshot, [CallerMemberName] string? name = null)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(nameof(Category), name!, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.Unit, snapshotName, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                if (snapshot is {})
                {
                    e.Push(new LogProperty(Names.Properties.Snapshot, snapshot, LogPropertyMeta.Builder.ProcessWith<Destructure>()));
                }
            });
        }

        // Templates

        public static Action<ILogEntry> Decision(this Action<ILogEntry> node, string decision, string? because = default)
        {
            return node.Flow().Unit(nameof(Decision)).Snapshot(decision).Message(because);
        }

        public static Action<ILogEntry> Unit(this Action<ILogEntry> node, string name)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Properties.Unit, name, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static Action<ILogEntry> Snapshot(this Action<ILogEntry> node, object snapshot)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Properties.Snapshot, snapshot, LogPropertyMeta.Builder.ProcessWith<SerializeProperty>())));
        }
        
        public static Action<ILogEntry> Flow(this Action<ILogEntry> node)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Properties.Category, nameof(Flow), LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        internal static Action<ILogEntry> BeginScope(this Action<ILogEntry> node, string name)
        {
            return node.Telemetry().Flow().Level(LogLevel.Information).Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Unit, nameof(BeginScope), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.Snapshot, new { name }, LogPropertyMeta.Builder.ProcessWith<SerializeProperty>()));
            });
        }

        internal static Action<ILogEntry> EndScope(this Action<ILogEntry> node, Exception? exception)
        {
            return node.Telemetry().Flow().Level(GetLogLevel(exception)).Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Unit, nameof(EndScope), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Push(new LogProperty(Names.Properties.Snapshot, new { status = GetFlowStatus(exception) }, LogPropertyMeta.Builder.ProcessWith<SerializeProperty>()));
            });

            static FlowStatus GetFlowStatus(Exception? exception)
            {
                return exception switch
                {
                    null => FlowStatus.Completed,
                    OperationCanceledException _ => FlowStatus.Canceled,
                    {} => FlowStatus.Faulted
                };
            }

            static LogLevel GetLogLevel(Exception? exception)
            {
                return exception switch
                {
                    null => LogLevel.Information,
                    OperationCanceledException e => LogLevel.Warning,
                    {} e => LogLevel.Error
                };
            }
        }

        private static ILogEntry Snapshot(this ILogEntry logEntry, object snapshot)
        {
            var dictionary = snapshot.ToDictionary();
            return
                logEntry
                    .Push(Names.Properties.Unit, dictionary.First().Key, m => m.ProcessWith<Echo>())
                    .Push(Names.Properties.Snapshot, dictionary.First().Value, m => m.ProcessWith<SerializeProperty>());
        }
    }
}