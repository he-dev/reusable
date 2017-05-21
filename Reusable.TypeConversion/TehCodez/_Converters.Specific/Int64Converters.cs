using System;
using System.Globalization;

namespace Reusable.TypeConversion
{
    public class StringToInt64Converter : TypeConverter<String, Int64>
    {
        protected override long ConvertCore(IConversionContext<string> context)
        {
            return Int64.Parse(context.Value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int64ToStringConverter : TypeConverter<long, string>
    {
        protected override string ConvertCore(IConversionContext<long> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
