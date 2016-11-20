using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class GeometricSequenceFactory
    {
        public static GeometricSequence<TimeSpan> Double(int count, TimeSpan first)
        {
            return new GeometricSequence<TimeSpan>(count, first, 2, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y)));
        }

        public static GeometricSequence<TimeSpan> Triple(int count, TimeSpan first)
        {
            return new GeometricSequence<TimeSpan>(count, first, 3, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y)));
        }

        public static GeometricSequence<TimeSpan> Halve(int count, TimeSpan first)
        {
            return new GeometricSequence<TimeSpan>(count, first, 0.5, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y)));
        }
    }
}


