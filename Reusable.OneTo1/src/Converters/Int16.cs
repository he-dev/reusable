using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt16 : TypeConverter<String, Int16>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public NumberStyles NumberStyles { get; set; } = NumberStyles.Integer;

        protected override short Convert(string value, ConversionContext context)
        {
            return Int16.Parse(value, NumberStyles, FormatProvider);
        }
    }

    public class Int16ToStringConverter : TypeConverter<short, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        protected override string Convert(short value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}