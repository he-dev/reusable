using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    public class Double : Constant<double>
    {
        public Double(string name, double value) : base(name, value) { }
    }

    public class Zero : Double
    {
        public Zero(string name) : base(name, 0.0) { }
    }

    public class One : Double
    {
        public One(string name) : base(name, 1.0) { }
    }

    public class True : Constant<bool>
    {
        public True(string name) : base(name, true) { }
    }

    public class False : Constant<bool>
    {
        public False(string name) : base(name, false) { }
    }

    public class String : Constant<string>
    {
        public String(string name, string value) : base(name, value) { }
    }

    public class Instant : Constant<DateTime>
    {
        public Instant(string name, string value, [CanBeNull] string format)
            : base
            (
                name,
                format is null
                    ? DateTime.Parse(value)
                    : DateTime.ParseExact(value, format, CultureInfo.InvariantCulture)
            ) { }
    }

    public class Interval : Constant<TimeSpan>
    {
        public Interval(string name, string value, [CanBeNull] string format)
            : base
            (
                name,
                format is null
                    ? TimeSpan.Parse(value)
                    : TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture)
            ) { }
    }
}