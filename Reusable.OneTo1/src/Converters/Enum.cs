using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToEnum : ITypeConverter
    {
        public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return value is string str && toType.IsEnum ? Enum.Parse(toType, str) : default;
        }
    }

    public class EnumToStringConverter : ITypeConverter
    {
        public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return value.GetType().IsEnum && toType == typeof(string) ? value.ToString() : default;
        }
    }
}