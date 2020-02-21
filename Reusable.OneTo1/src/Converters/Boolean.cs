namespace Reusable.OneTo1.Converters
{
    public class StringToBoolean : FromStringConverter<bool>
    {
        protected override bool Convert(string value, ConversionContext context)
        {
            return bool.Parse(value);
        }
    }

    public class BooleanToString : ToStringConverter<bool>
    {
        protected override string Convert(bool value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}