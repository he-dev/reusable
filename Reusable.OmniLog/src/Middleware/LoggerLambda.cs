using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public delegate void AlterLog(Log log);
    
    public class LoggerLambda : LoggerMiddleware
    {
        public LoggerLambda() : base(false) { }
        
        public override bool IsActive => !(LoggerScope<Item>.Current is null);

        public static void Push(Item item) => LoggerScope<Item>.Push(item);

        protected override void InvokeCore(Log request)
        {
            while (IsActive)
            {
                using (var item = LoggerScope<Item>.Current.Value)
                {
                    item.Alter(request);
                }
            }

            Next?.Invoke(request);
        }

        public class Item : IDisposable
        {
            public AlterLog Alter { get; set; }

            public void Dispose() => LoggerScope<Item>.Current.Dispose();
        }
    }
}