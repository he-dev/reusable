using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToSingle : TypeConverter<String, Single>
    {
        protected override Single Convert(string value, ConversionContext context)
        {
            return Single.Parse(value, context.FormatProvider);
        }
    }

    public class SingleToStringConverter : TypeConverter<float, string>
    {
        protected override string Convert(float value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? value.ToString(context.FormatProvider)
                    : value.ToString(context.FormatString, context.FormatProvider);
        }
    }
}