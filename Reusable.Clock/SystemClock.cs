using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class SystemClock : IClock
    {
        public DateTime GetNow() => DateTime.Now;
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
