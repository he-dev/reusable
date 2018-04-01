using System;
using System.Diagnostics;
using System.Reactive;


using Reusable.OmniLog.Collections;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class DebugRx : LogRx
    {
        private DebugRx() { }

        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(log =>
            {
                Debug.WriteLine(log);
            });
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}