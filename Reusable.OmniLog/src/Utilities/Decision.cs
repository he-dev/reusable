using System.Collections.Generic;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities {
    public static class Decision
    {
        public static IEnumerable<LogProperty> Make(string decision, string? because = default)
        {
            yield return new LogProperty(Names.Default.Category, nameof(ExecutionCategories.Flow), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.SnapshotName, nameof(Decision), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            if (because is {})
            {
                yield return new LogProperty(Names.Default.Message, because, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            }
        }
    }
}