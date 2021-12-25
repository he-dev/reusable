using System;

namespace Reusable.Snowball.Converters.Specialized;

public class Always : ITypeConverter
{
    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default) => value;
}