using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class TestClock : IClock
    {
        public DateTime Now { get; set; }
        public DateTime UtcNow { get; set; }
        DateTime IClock.GetNow() => Now;
        DateTime IClock.GetUtcNow() => UtcNow;
    }
}
