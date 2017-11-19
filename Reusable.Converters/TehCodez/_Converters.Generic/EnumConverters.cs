using System;

namespace Reusable.Converters
{
    public class StringToEnumConverter : TypeConverter<String, object>
    {
        public override bool CanConvert(Type fromType, Type toType)
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
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnum && toType == typeof(string);
        }

        protected override string ConvertCore(IConversionContext<Enum> context)
        {
            return context.Value.ToString();
        }
    }
}
