using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToGuidConverter : TypeConverter<String, Guid>
    {
        protected override Guid Convert(string value, ConversionContext context)
        {
            return Guid.Parse(value);
        }
    }

    public class GuidToStringConverter : TypeConverter<Guid, String>
    {
        protected override string Convert(Guid value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? value.ToString()
                    : value.ToString(context.FormatString, context.FormatProvider);
        }
    }
}
