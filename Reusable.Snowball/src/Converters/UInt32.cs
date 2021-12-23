using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt32 : TypeConverter<String, UInt32>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override UInt32 Convert(string value, ConversionContext context)
        {
            return UInt32.Parse(value, FormatProvider);
        }
    }

    public class UInt32ToStringConverter : TypeConverter<uint, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override string Convert(uint value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}
