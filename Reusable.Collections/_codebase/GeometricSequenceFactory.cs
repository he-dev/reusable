using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class GeometricSequenceFactory
    {
        public static GeometricSequence<TimeSpan> Double(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, x => TimeSpan.FromTicks(x.Ticks * 2), count);
        }

        public static GeometricSequence<TimeSpan> Triple(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, x => TimeSpan.FromTicks(x.Ticks * 3), count);
        }

        public static GeometricSequence<TimeSpan> Halve(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, x => TimeSpan.FromTicks(x.Ticks / 2), count);
        }
    }
}


