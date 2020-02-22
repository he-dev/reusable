using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToSByte : TypeConverter<String, SByte>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        protected override SByte Convert(string value, ConversionContext context)
        {
            return SByte.Parse(value, FormatProvider);
        }
    }

    public class SByteToStringConverter : TypeConverter<sbyte, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        protected override string Convert(sbyte value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}
