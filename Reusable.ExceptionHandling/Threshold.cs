using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Threshold
    {
        public Threshold(int count, TimeSpan interval)
        {
            Count = count;
            Interval = interval;
        }

        public int Count { get; }

        public TimeSpan Interval { get; }

        public override string ToString()
        {
            return $"Count = {Count} Interval = {Interval}";
        }
    }
}
