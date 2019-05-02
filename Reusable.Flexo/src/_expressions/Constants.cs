using System.Globalization;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

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
        public String() : base(nameof(String), default) { }
    }

    public class DateTime : Constant<System.DateTime>
    {
        public DateTime() : base(nameof(DateTime), default) { }

        public string Parse { get; set; }

        public string Format { get; set; }

        protected override Constant<System.DateTime> InvokeCore(IImmutableSession context)
        {
            var value =
                Format is null
                    ? System.DateTime.Parse(Parse)
                    : System.DateTime.ParseExact(Parse, Format, CultureInfo.InvariantCulture);
            return (Name, value, context);
        }
    }

    public class TimeSpan : Constant<System.TimeSpan>
    {
        public TimeSpan() : base(nameof(TimeSpan), default) { }
        
        public string Parse { get; set; }

        public string Format { get; set; }
        
        protected override Constant<System.TimeSpan> InvokeCore(IImmutableSession context)
        {
            var value =
                Format is null
                    ? System.TimeSpan.Parse(Parse)
                    : System.TimeSpan.ParseExact(Parse, Format, CultureInfo.InvariantCulture);
            return (Name, value, context);
        }
    }
}