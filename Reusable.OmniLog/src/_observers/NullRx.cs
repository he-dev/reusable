using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class NullRx : LogRx
    {
        public override void Log(ILog log) { }
    }
}