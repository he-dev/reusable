using System;
using System.Windows;

namespace Reusable.Windows
{
    public static class PropertyMetadataBuilderExtensions
    {
        public static PropertyMetadataBuilder<T, TValue> CoerceValue<T, TValue>(
            this PropertyMetadataBuilder<T, TValue> builder,
            Action<T, CoerceValueEventArgs<TValue>> coerceValueCallback
        ) where T : DependencyObject
        {
            builder.CoerceValue((d, baseValue) =>
            {
                var e = new CoerceValueEventArgs<TValue>(baseValue);
                coerceValueCallback(d, e);
                return
                    e.Canceled
                        ? DependencyProperty.UnsetValue
                        : e.CoercedValue;
            });
            return builder;
        }
    }
}