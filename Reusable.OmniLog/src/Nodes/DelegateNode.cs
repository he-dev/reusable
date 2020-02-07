using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class DelegateNode : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Item item) => AsyncScope<Item>.Push(item);

        protected override void invoke(ILogEntry request)
        {
            while (Enabled)
            {
                using var item = AsyncScope<Item>.Current!.Value!;
                item.AlterLogEntry(request);
            }

            invokeNext(request);
        }

        public class Item : IDisposable
        {
            public Item(AlterLogEntryDelegate alterLogEntry)
            {
                AlterLogEntry = alterLogEntry;
            }
            
            public AlterLogEntryDelegate AlterLogEntry { get; }

            public void Dispose() => AsyncScope<Item>.Current?.Dispose();
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseDelegate(this ILogger logger, AlterLogEntryDelegate alter)
        {
            DelegateNode.Push(new DelegateNode.Item(alter));
        }
    }
}