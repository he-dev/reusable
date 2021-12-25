using System;

namespace Reusable.Snowball.Converters.Specialized;

public class Lambda : ITypeConverter
{
    private readonly Func<object, Type, ConversionContext?, object?> _convertOrDefault;

    public Lambda(Func<object, Type, ConversionContext?, object?> convertOrDefault)
    {
        _convertOrDefault = convertOrDefault;
    }

    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        return _convertOrDefault(value, toType, context);
    }
}