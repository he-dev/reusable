﻿using System;

namespace Reusable.Sequences
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


