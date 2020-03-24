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
    public static partial class ExecutionCategories
    {
        [Obsolete("Use other overloads.")]
        public static Action<ILogEntry> Variable(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Variable), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        [Obsolete("Use other overloads.")]
        public static Action<ILogEntry> Counter(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Counter), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        [Obsolete("Use other overloads.")]
        public static Action<ILogEntry> Meta(this Action<ILogEntry> node, object value)
        {
            return node.Then(e =>
            {
                e.Push(new LogProperty(Names.Properties.Category, nameof(Meta), LogPropertyMeta.Builder.ProcessWith<Echo>()));
                e.Snapshot(value);
            });
        }

        [Obsolete("Use other overloads.")]
        public static Action<ILogEntry> Because(this Action<ILogEntry> node, string because)
        {
            return node.Then(e => { e.Message(because); });
        }
    }
}