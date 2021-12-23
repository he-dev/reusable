using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToBoolean : TypeConverter<string, bool>
    {
        protected override bool Convert(string value, ConversionContext context)
        {
            return bool.Parse(value);
        }
    }

    public class BooleanToString : TypeConverter<bool, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override string Convert(bool value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}