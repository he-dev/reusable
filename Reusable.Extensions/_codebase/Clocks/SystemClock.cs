using System;

namespace Reusable.Clocks
{
    public class SystemClock : IClock
    {
        public DateTime GetNow() => DateTime.Now;
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
