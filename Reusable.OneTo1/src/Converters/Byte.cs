using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToByteConverter : TypeConverter<String, Byte>
    {
        protected override byte ConvertCore(IConversionContext<string> context)
        {
            return byte.Parse(context.Value);
        }
    }

    public class ByteToStringConverter : TypeConverter<Byte, String>
    {
        protected override string ConvertCore(IConversionContext<byte> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
