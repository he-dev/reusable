using System;
using System.Windows;

namespace Reusable.Windows
{
    public class PropertyMetadataBuilder<T, TValue> where T : DependencyObject
    {
        private readonly PropertyMetadata _propertyMetadata;

        internal PropertyMetadataBuilder()
        {
            _propertyMetadata = new PropertyMetadata();
        }

        public PropertyMetadataBuilder<T, TValue> DefaultValue(
            TValue defaultValue
        )
        {
            _propertyMetadata.DefaultValue = defaultValue;
            return this;
        }

        public PropertyMetadataBuilder<T, TValue> PropertyChanged(
            Action<T, DependencyPropertyChangedEventArgs<TValue>> propertyChangedCallback
        )
        {
            _propertyMetadata.PropertyChangedCallback = (sender, e) =>
                propertyChangedCallback(
                    (T)sender,
                    new DependencyPropertyChangedEventArgs<TValue>(e)
                );
            return this;
        }

        public PropertyMetadataBuilder<T, TValue> CoerceValue(
            Func<T, TValue, object> coerceValueCallback
        )
        {
            _propertyMetadata.CoerceValueCallback = (d, baseValue) => 
                coerceValueCallback((T) d, (TValue)baseValue);
            return this;
        }

        public static implicit operator PropertyMetadata(PropertyMetadataBuilder<T, TValue> builder)
            => builder._propertyMetadata;
    }
}