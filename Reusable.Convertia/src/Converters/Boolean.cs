namespace Reusable.Convertia.Converters
{
    public class StringToBooleanConverter : TypeConverter<string, bool>
    {
        protected override bool ConvertCore(IConversionContext<string> context)
        {
            return bool.Parse(context.Value);
        }
    }

    public class BooleanToStringConverter : TypeConverter<bool, string>
    {
        protected override string ConvertCore(IConversionContext<bool> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
