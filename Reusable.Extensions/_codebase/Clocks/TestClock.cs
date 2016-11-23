using System;

namespace Reusable.Clocks
{
    public class TestClock : IClock
    {
        public DateTime Now { get; set; }
        public DateTime UtcNow { get; set; }
        DateTime IClock.GetNow() => Now;
        DateTime IClock.GetUtcNow() => UtcNow;
    }
}
