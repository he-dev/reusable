using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerLambda : LoggerMiddleware //, ILoggerQueue<LoggerLambda.Item>
    {
        public LoggerLambda() : base(false) { }
        
        public override bool IsActive => !(LoggerScope<Item>.Current is null);

        public static void Push(Item item) => LoggerScope<Item>.Push(item);

        protected override void InvokeCore(ILog request)
        {
            while (IsActive)
            {
                using (var item = LoggerScope<Item>.Current.Value)
                {
                    item.Transform(request);
                }
            }

            Next?.Invoke(request);
        }

        public class Item : IDisposable
        {
            public Action<ILog> Transform { get; set; }

            public void Dispose() => LoggerScope<Item>.Current.Dispose();
        }
    }
}