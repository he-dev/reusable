using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Double : Constant<double>
    {
        public Double() : base(nameof(Double), default) { }

        public static readonly Constant<double> Zero = new Double { Name = nameof(Zero), Value = 0.0 };

        public static readonly Constant<double> One = new Double { Name = nameof(One), Value = 1.0 };
    }

    public class Integer : Constant<int>
    {
        public Integer(string name, int value) : base(name ?? nameof(Integer), value) { }

        public static readonly Integer Zero = new Integer(nameof(Zero), 0);

        public static readonly Integer One = new Integer(nameof(One), 1);
    }

    public class Decimal : Constant<decimal>
    {
        public Decimal(string name, decimal value) : base(name ?? nameof(Decimal), value) { }

        public static readonly Decimal Zero = new Decimal(nameof(Zero), 0m);

        public static readonly Decimal One = new Decimal(nameof(One), 1m);
    }

    //    public class Zero : Double
    //    {
    //        public Zero(string name) : base(name, 0.0) { }
    //    }
    //
    //    public class One : Double
    //    {
    //        public One(string name) : base(name, 1.0) { }
    //    }

    public class True : Constant<bool>
    {
        public True() : base(nameof(True), true) { }
    }

    public class False : Constant<bool>
    {
        public False() : base(nameof(False), false) { }
    }

    public class String : Constant<string>
    {
        public String(string name, string value) : base(name ?? nameof(String), value) { }
    }

    public class DateTime : Constant<System.DateTime>
    {
        public DateTime(string name, string value, [CanBeNull] string format)
            : base
            (
                name ?? nameof(DateTime),
                format is null
                    ? System.DateTime.Parse(value)
                    : System.DateTime.ParseExact(value, format, CultureInfo.InvariantCulture)
            ) { }
    }

    public class TimeSpan : Constant<System.TimeSpan>
    {
        public TimeSpan(string name, string value, [CanBeNull] string format)
            : base
            (
                name ?? nameof(TimeSpan),
                format is null
                    ? System.TimeSpan.Parse(value)
                    : System.TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture)
            ) { }
    }
}