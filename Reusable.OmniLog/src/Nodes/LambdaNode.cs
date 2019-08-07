using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class LambdaNode : LoggerNode
    {
        public LambdaNode() : base(false) { }

        public override bool Enabled => LoggerScope<Item>.Any;

        public static void Push(Item item) => LoggerScope<Item>.Push(item);

        protected override void InvokeCore(LogEntry request)
        {
            while (Enabled)
            {
                using (var item = LoggerScope<Item>.Current.Value)
                {
                    item.AlterLogEntry(request);
                }
            }

            Next?.Invoke(request);
        }

        public class Item : IDisposable
        {
            public AlterLogEntryCallback AlterLogEntry { get; set; }

            public void Dispose() => LoggerScope<Item>.Current.Dispose();
        }
    }

    public delegate void AlterLogEntryCallback(LogEntry logEntry);
    
    public static class LoggerLambdaHelper
    {
        public static void UseLambda(this ILogger logger, AlterLogEntryCallback alter)
        {
            LambdaNode.Push(new LambdaNode.Item { AlterLogEntry = alter });
        }
    }
}