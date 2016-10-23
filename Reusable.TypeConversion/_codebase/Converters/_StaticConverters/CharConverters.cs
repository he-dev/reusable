using System;

namespace Reusable.Converters
{
    public class StringToCharConverter : StaticConverter<String, Char>
    {
        public override Char Convert(string value, ConversionContext context)
        {
            return Char.Parse(value);
        }
    }

    public class CharToStringConverter : StaticConverter<char, string>
    {
        public override string Convert(Char value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
