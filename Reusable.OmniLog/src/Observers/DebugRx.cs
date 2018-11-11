using System;
using System.Diagnostics;
using System.Reactive;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class DebugRx : LogRx
    {
        private DebugRx() { }

        protected override void Log(Log log)
        {
            Debug.WriteLine(log);
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}