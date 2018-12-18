using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToCharConverter : TypeConverter<String, Char>
    {
        protected override char ConvertCore(IConversionContext<string> context)
        {
            return char.Parse(context.Value);
        }
    }

    public class CharToStringConverter : TypeConverter<char, string>
    {
        protected override string ConvertCore(IConversionContext<char> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
