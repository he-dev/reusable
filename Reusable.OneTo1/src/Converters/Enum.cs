using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToEnum : TypeConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(string) && toType.IsEnum;
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            return Enum.Parse(toType, (string)value);
        }
    }

    public class EnumToStringConverter : TypeConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnum && toType == typeof(string);
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            return value.ToString();
        }
    }
}