using System;

namespace Reusable.Convertia.Converters
{
    public class StringToSingleConverter : TypeConverter<String, Single>
    {
        protected override Single ConvertCore(IConversionContext<String> context)
        {
            return Single.Parse(context.Value, context.FormatProvider);
        }
    }

    public class SingleToStringConverter : TypeConverter<float, string>
    {
        protected override string ConvertCore(IConversionContext<Single> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
