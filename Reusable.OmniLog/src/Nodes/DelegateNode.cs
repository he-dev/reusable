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
            while (AsyncScope<Item>.Current is {} current)
            {
                using (current)
                {
                    current.Value.ProcessLogEntry(request);
                }
            }

            InvokeNext(request);
        }

        public class Item
        {
            public Item(ProcessLogEntryDelegate processLogEntry)
            {
                ProcessLogEntry = processLogEntry;
            }

            public ProcessLogEntryDelegate ProcessLogEntry { get; }
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseDelegate(this ILogger logger, ProcessLogEntryDelegate process)
        {
            DelegateNode.Push(new DelegateNode.Item(process));
        }
    }
}