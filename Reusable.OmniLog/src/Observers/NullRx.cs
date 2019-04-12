using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class NullRx : LogRx
    {
        protected override void Log(ILog log) { }
    }
}