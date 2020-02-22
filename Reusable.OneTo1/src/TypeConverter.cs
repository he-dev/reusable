using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    public interface ITypeConverter
    {
        object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);
    }

    public abstract class TypeConverter<TValue, TResult> : ITypeConverter
    {
        public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                value is TValue x && toType == typeof(TResult)
                    ? Convert(x, context ?? new ConversionContext { Converter = this })
                    : default;
        }

        protected abstract TResult Convert(TValue value, ConversionContext context);
    }
}