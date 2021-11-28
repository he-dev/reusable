using System;

namespace Reusable.OneTo1
{
    public interface ITypeConverter
    {
        object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);
    }

    public abstract class  TypeConverter<TValue, TResult> : ITypeConverter
    {
        public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                //value is TValue x && toType == typeof(TResult)
                value is TValue x && toType.IsAssignableFrom(typeof(TResult))
                //typeof(TValue).IsAssignableFrom(value.GetType()) && toType == typeof(TResult)
                //value.GetType().IsAssignableFrom(typeof(TValue)) && toType == typeof(TResult)
                    ? Convert(x, context ?? new ConversionContext())
                    : default(object);
        }

        protected abstract TResult Convert(TValue value, ConversionContext context);
    }
}