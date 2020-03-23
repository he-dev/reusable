using System.Collections.Generic;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities.Logging
{
    public static class Decision
    {
        public static IEnumerable<LogProperty> Make(string decision, string? because = default)
        {
            yield return new LogProperty(Names.Properties.Category, nameof(ExecutionCategories.Flow), LogPropertyMeta.Builder.ProcessWith<Echo>());
            yield return new LogProperty(Names.Properties.Unit, nameof(Decision), LogPropertyMeta.Builder.ProcessWith<Echo>());
            yield return new LogProperty(Names.Properties.Snapshot, decision, LogPropertyMeta.Builder.ProcessWith<Echo>());
            if (because is {})
            {
                yield return new LogProperty(Names.Properties.Message, because, LogPropertyMeta.Builder.ProcessWith<Echo>());
            }
        }
    }
}