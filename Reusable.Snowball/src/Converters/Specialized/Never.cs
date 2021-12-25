using System;

namespace Reusable.Snowball.Converters.Specialized;

public class Never : ITypeConverter
{
    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default) => default;
}