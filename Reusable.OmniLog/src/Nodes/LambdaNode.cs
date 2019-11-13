using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class LambdaNode : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Item item) => AsyncScope<Item>.Push(item);

        protected override void invoke(LogEntry request)
        {
            while (Enabled)
            {
                using (var item = AsyncScope<Item>.Current.Value)
                {
                    item?.AlterLogEntry(request);
                }
            }

            invokeNext(request);
        }

        public class Item : IDisposable
        {
            public AlterLogEntryCallback? AlterLogEntry { get; set; }

            public void Dispose() => AsyncScope<Item>.Current?.Dispose();
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseLambda(this ILogger logger, AlterLogEntryCallback alter)
        {
            LambdaNode.Push(new LambdaNode.Item { AlterLogEntry = alter });
        }
    }
}