using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.OneTo1.Converters
{
    public class StringToByte : TypeConverter<string, byte>
    {
        protected override byte Convert(string value, ConversionContext context)
        {
            return byte.Parse(value);
        }
    }

    public class ByteToStringConverter : TypeConverter<byte, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
        protected override string Convert(byte value, ConversionContext context)
        {
            return value.ToString(FormatProvider);
        }
    }
}