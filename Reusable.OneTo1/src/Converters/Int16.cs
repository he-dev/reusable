using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt16Converter : TypeConverter<String, Int16>
    {
        protected override short Convert(IConversionContext<string> context)
        {
            return Int16.Parse(context.Value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int16ToStringConverter : TypeConverter<short, string>
    {
        protected override string Convert(IConversionContext<short> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
