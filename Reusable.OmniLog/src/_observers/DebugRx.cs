using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class DebugRx : ILogRx
    {
        private DebugRx() { }

        public void Log(Log log)
        {
            Debug.WriteLine(log);
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}