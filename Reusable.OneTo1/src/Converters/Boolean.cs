namespace Reusable.OneTo1.Converters
{
    public class StringToBooleanConverter : TypeConverter<string, bool>
    {
        protected override bool Convert(IConversionContext<string> context)
        {
            return bool.Parse(context.Value);
        }
    }

    public class BooleanToStringConverter : TypeConverter<bool, string>
    {
        protected override string Convert(IConversionContext<bool> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
