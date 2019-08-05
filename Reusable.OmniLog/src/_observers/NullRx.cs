using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    public class NullRx : ILogRx
    {
        public void Log(Log log) { }
    }
}