using System.Collections.Generic;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities
{
    public static class Snapshot
    {
        public static IEnumerable<LogProperty> Take(string name, object dump)
        {
            yield return new LogProperty(Names.Default.SnapshotName, name, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.Snapshot, dump.ToDictionary(), LogPropertyMeta.Builder.ProcessWith<SerializerNode>());
        }
    }
}