using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class ExecutionMessages
    {
        public static Action<ILogEntry> Message(this Action<ILogEntry> node, string message)
        {
            return node.Then(e => e.Push(new LogProperty(Names.Default.Message, message, LogPropertyMeta.Builder.ProcessWith<EchoNode>())));
        }
    }
}