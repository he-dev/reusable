using System;

namespace Reusable.OneTo1.Decorators
{
    public class SkipConverted : ITypeConverter, IDecorator<ITypeConverter>
    {
        public SkipConverted(ITypeConverter converter) => Decoratee = converter;

        public ITypeConverter Decoratee { get; }

        public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            //toType.IsInstanceOfType(value)
            return value.GetType().IsAssignableFrom(toType) ? value : Decoratee.ConvertOrDefault(value, toType, context);
        }
    }
}