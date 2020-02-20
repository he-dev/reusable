using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt32Converter : TypeConverter<String, UInt32>
    {
        protected override UInt32 Convert(IConversionContext<String> context)
        {
            return UInt32.Parse(context.Value, context.FormatProvider);
        }
    }

    public class UInt32ToStringConverter : TypeConverter<uint, string>
    {
        protected override string Convert(IConversionContext<UInt32> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
