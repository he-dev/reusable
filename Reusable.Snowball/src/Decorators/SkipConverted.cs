using System;
using Reusable.Essentials;

namespace Reusable.Snowball.Decorators;

public class SkipConverted : ITypeConverter, IDecorator<ITypeConverter>
{
    public SkipConverted(ITypeConverter converter) => Decoratee = converter;

    public ITypeConverter Decoratee { get; }

    public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        return value.GetType().IsAssignableFrom(toType) ? value : Decoratee.ConvertOrDefault(value, toType, context);
    }
}