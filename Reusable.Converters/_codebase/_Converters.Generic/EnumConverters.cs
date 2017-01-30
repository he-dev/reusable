using System;

namespace Reusable.Converters
{
    public class StringToEnumConverter : TypeConverter<String, object>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return value is string && targetType.IsEnum;
        }

        protected override object ConvertCore(IConversionContext<string> context)
        {
            return Enum.Parse(context.TargetType, context.Value);
        }
    }

    public class EnumToStringConverter : TypeConverter<Enum, string>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return value.GetType().IsEnum && targetType == typeof(string);
        }

        protected override string ConvertCore(IConversionContext<Enum> context)
        {
            return context.Value.ToString();
        }
    }
}
