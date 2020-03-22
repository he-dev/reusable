using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities
{
    public static class CallSite
    {
        public static IEnumerable<LogProperty> Create
        (
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            yield return new LogProperty(Names.Properties.CallerMemberName, callerMemberName!, LogPropertyMeta.Builder.ProcessWith<Echo>());
            yield return new LogProperty(Names.Properties.CallerLineNumber, callerLineNumber, LogPropertyMeta.Builder.ProcessWith<Echo>());
            yield return new LogProperty(Names.Properties.CallerFilePath, Path.GetFileName(callerFilePath!), LogPropertyMeta.Builder.ProcessWith<Echo>());
        }
    }
}