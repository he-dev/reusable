using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToEnumConverter : TypeConverter<String, object>
    {
        protected override bool SupportsConversion(Type fromType, Type toType)
        {
            return fromType == typeof(string) && toType.IsEnum;
        }

        protected override object ConvertCore(IConversionContext<string> context)
        {
            return Enum.Parse(context.ToType, context.Value);
        }
    }

    public class EnumToStringConverter : TypeConverter<Enum, string>
    {
        protected override bool SupportsConversion(Type fromType, Type toType)
        {
            return fromType.IsEnum && toType == typeof(string);
        }

        protected override string ConvertCore(IConversionContext<Enum> context)
        {
            return context.Value.ToString();
        }
    }
}
