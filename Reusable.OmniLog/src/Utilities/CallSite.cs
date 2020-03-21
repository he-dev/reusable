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
            yield return new LogProperty(Names.Default.CallerMemberName, callerMemberName!, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.CallerLineNumber, callerLineNumber, LogPropertyMeta.Builder.ProcessWith<EchoNode>());
            yield return new LogProperty(Names.Default.CallerFilePath, Path.GetFileName(callerFilePath!), LogPropertyMeta.Builder.ProcessWith<EchoNode>());
        }
    }
}