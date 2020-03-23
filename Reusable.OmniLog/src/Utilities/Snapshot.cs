using System.Collections.Generic;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities
{
    public static class Snapshot
    {
        public static IEnumerable<LogProperty> Take(string name, object dump)
        {
            yield return new LogProperty(Names.Properties.Unit, name, LogPropertyMeta.Builder.ProcessWith<Echo>());
            yield return new LogProperty(Names.Properties.Snapshot, dump.ToDictionary(), LogPropertyMeta.Builder.ProcessWith<SerializeProperty>());
        }
    }
}