using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Threshold
    {
        public Threshold(int count, TimeSpan interval, TimeSpan timeout)
        {
            Count = count;
            Interval = interval;
            Timeout = timeout;
        }

        public Threshold(int count, TimeSpan interval)
            : this(count, interval, TimeSpan.Zero)
        { }

        public int Count { get; }

        public TimeSpan Interval { get; }

        public TimeSpan Timeout { get; }

        public override string ToString()
        {
            return $"Count = {Count} Interval = {Interval} Timeout = {Timeout}";
        }
    }
}
