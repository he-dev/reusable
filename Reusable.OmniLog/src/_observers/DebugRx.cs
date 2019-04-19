using System.Diagnostics;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class DebugRx : LogRx
    {
        private DebugRx() { }

        protected override void Log(ILog log)
        {
            Debug.WriteLine(log);
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}