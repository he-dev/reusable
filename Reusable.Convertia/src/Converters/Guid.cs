using System;

namespace Reusable.Convertia.Converters
{
    public class StringToGuidConverter : TypeConverter<String, Guid>
    {
        protected override Guid ConvertCore(IConversionContext<string> context)
        {
            return Guid.Parse(context.Value);
        }
    }

    public class GuidToStringConverter : TypeConverter<Guid, String>
    {
        protected override string ConvertCore(IConversionContext<Guid> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString()
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
