using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.OneTo1.Converters
{
    public class StringToByte : FromStringConverter<byte>
    {
        protected override byte Convert(string value, ConversionContext context)
        {
            return byte.Parse(value);
        }
    }

    public class ByteToStringConverter : ToStringConverter<byte>
    {
        protected override string Convert(byte value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}