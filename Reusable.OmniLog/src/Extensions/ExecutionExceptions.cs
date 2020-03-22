using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class ExecutionExceptions
    {
        public static Action<ILogEntry> Exception(this Action<ILogEntry> node, Exception exception)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Default.Exception, exception, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }
    }
}