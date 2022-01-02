using System;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Snowball.Decorators;

public class FriendlyException : ITypeConverter, IDecorator<ITypeConverter>
{
    public FriendlyException(ITypeConverter converter) => Decoratee = converter;

    public ITypeConverter Decoratee { get; }
        
    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        try
        {
            return Decoratee.ConvertOrDefault(value, toType, context ?? new ConversionContext());
        }
        catch (Exception inner)
        {
            throw DynamicException.Create
            (
                $"TypeConversion",
                $"{Decoratee.GetType().ToPrettyString()} could not convert from '{value.GetType().ToPrettyString()}' to '{toType.ToPrettyString()}'. See the inner exception for details.",
                inner
            );
        }
    }

}