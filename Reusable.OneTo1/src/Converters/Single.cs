using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToSingleConverter : TypeConverter<String, Single>
    {
        protected override Single Convert(IConversionContext<String> context)
        {
            return Single.Parse(context.Value, context.FormatProvider);
        }
    }

    public class SingleToStringConverter : TypeConverter<float, string>
    {
        protected override string Convert(IConversionContext<Single> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
