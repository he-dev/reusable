using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt64 : TypeConverter<String, UInt64>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override UInt64 Convert(string value, ConversionContext context)
        {
            return UInt64.Parse(value, FormatProvider);
        }
    }

    public class UInt64ToStringConverter : TypeConverter<ulong, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override string Convert(ulong value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}
