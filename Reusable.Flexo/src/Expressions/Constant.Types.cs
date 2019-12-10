using System.Globalization;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Double : Constant<double>
    {
        [JsonConstructor]
        public Double(string? id, double value) : base(id ?? nameof(Double), new[] { value }) { }

        public static readonly Constant<double> Zero = new Double(nameof(Zero), 0.0);

        public static readonly Constant<double> One = new Double(nameof(One), 1.0);
    }

    public class Integer : Constant<int>
    {
        public Integer(string name, int value) : base(name ?? nameof(Integer), new[] { value }) { }

        public static readonly Integer Zero = new Integer(nameof(Zero), 0);

        public static readonly Integer One = new Integer(nameof(One), 1);
    }

    public class Decimal : Constant<decimal>
    {
        public Decimal(string name, decimal value) : base(name ?? nameof(Decimal), new[] { value }) { }

        public static readonly Decimal Zero = new Decimal(nameof(Zero), 0m);

        public static readonly Decimal One = new Decimal(nameof(One), 1m);
    }

    public class True : Constant<bool>
    {
        [JsonConstructor]
        public True(string? id) : base(id ?? nameof(True), new[] { true }) { }
    }

    public class False : Constant<bool>
    {
        [JsonConstructor]
        public False(string? id) : base(id ?? nameof(False), new[] { false }) { }
    }

    public class String : Constant<string>
    {
        [JsonConstructor]
        public String(string? id, string value) : base(id ?? nameof(String), new[] { value }) { }
    }

    public class DateTime : Constant<System.DateTime>
    {
        [JsonConstructor]
        public DateTime(string? id, string value, string? format) : base(id ?? nameof(DateTime), new[] { Parse(value, format) }) { }

        private static System.DateTime Parse(string value, string? format)
        {
            return
                string.IsNullOrEmpty(format)
                    ? System.DateTime.Parse(value)
                    : System.DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
    }

    public class TimeSpan : Constant<System.TimeSpan>
    {
        [JsonConstructor]
        public TimeSpan(string? id, string value, string? format) : base(id ?? nameof(TimeSpan), new[] { Parse(value, format) }) { }

        private static System.TimeSpan Parse(string value, string? format)
        {
            return
                string.IsNullOrEmpty(format)
                    ? System.TimeSpan.Parse(value)
                    : System.TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
    }
}