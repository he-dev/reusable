using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt32Converter : TypeConverter<String, Int32>
    {
        protected override int Convert(IConversionContext<string> context)
        {
            return Int32.Parse(context.Value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int32ToStringConverter : TypeConverter<Int32, String>
    {
        protected override string Convert(IConversionContext<int> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
