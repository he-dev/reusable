using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node adds support for <c>Logger.Log(log => ..)</c> overload.
    /// </summary>
    public class InjectAnonymousAction : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Action<ILogEntry> node) => AsyncScope<Item>.Push(new Item(node));

        public override void Invoke(ILogEntry request)
        {
            while (AsyncScope<Item>.Current is {} current)
            {
                using (current)
                {
                    current.Value.Action(request);
                }
            }

            InvokeNext(request);
        }

        private class Item
        {
            public Item(Action<ILogEntry> node) => Action = node;

            public Action<ILogEntry> Action { get; }
        }
    }

    public static class LoggerLambdaHelper
    {
        public static ILogger PushDelegate(this ILogger logger, Action<ILogEntry> action)
        {
            return logger.Also(_ => InjectAnonymousAction.Push(action));
        }
    }
}