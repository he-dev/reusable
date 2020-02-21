using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToChar : FromStringConverter<char>
    {
        protected override char Convert(string value, ConversionContext context)
        {
            return char.Parse(value);
        }
    }

    public class CharToStringConverter : ToStringConverter<char>
    {
        protected override string Convert(char value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}