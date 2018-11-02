using System;

namespace Reusable.Convertia.Converters
{
    public class StringToUInt64Converter : TypeConverter<String, UInt64>
    {
        protected override UInt64 ConvertCore(IConversionContext<String> context)
        {
            return UInt64.Parse(context.Value, context.FormatProvider);
        }
    }

    public class UInt64ToStringConverter : TypeConverter<ulong, string>
    {
        protected override string ConvertCore(IConversionContext<UInt64> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
