using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds support for logger.Log(log => ..) overload.
    /// </summary>
    public class InjectAnonymousDelegate : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Action<ILogEntry> node) => AsyncScope<Item>.Push(new Item(node));

        public override void Invoke(ILogEntry request)
        {
            while (AsyncScope<Item>.Current is {} current)
            {
                using (current)
                {
                    current.Value.Node(request);
                }
            }

            InvokeNext(request);
        }

        private class Item
        {
            public Item(Action<ILogEntry> node) => Node = node;

            public Action<ILogEntry> Node { get; }
        }
    }

    public static class LoggerLambdaHelper
    {
        public static ILogger PushDelegate(this ILogger logger, Action<ILogEntry> node)
        {
            return logger.Pipe(_ => InjectAnonymousDelegate.Push(node));
        }
    }
}