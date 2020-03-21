using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class DelegateNode : LoggerNode
    {
        public override bool Enabled => AsyncScope<Item>.Any;

        public static void Push(Item item) => AsyncScope<Item>.Push(item);

        public override void Invoke(ILogEntry request)
        {
            while (Enabled)
            {
                using var scope = AsyncScope<Item>.Current;
                scope.Value.AlterLogEntry(request);
            }

            InvokeNext(request);
        }

        public class Item
        {
            public Item(AlterLogEntryDelegate alterLogEntry)
            {
                AlterLogEntry = alterLogEntry;
            }

            public AlterLogEntryDelegate AlterLogEntry { get; }
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