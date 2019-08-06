using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public delegate void AlterLogEntryCallback(LogEntry logEntry);
    
    public class LoggerLambda : LoggerMiddleware
    {
        public LoggerLambda() : base(false) { }
        
        public override bool IsActive => !LoggerScope<Item>.IsEmpty;

        public static void Push(Item item) => LoggerScope<Item>.Push(item);

        protected override void InvokeCore(LogEntry request)
        {
            while (IsActive)
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
}