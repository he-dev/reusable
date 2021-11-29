using System.Collections.Generic;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Utilities.Logging
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