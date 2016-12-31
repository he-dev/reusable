using System;
using System.Windows;

namespace Reusable.Windows
{
    public static class DependencyPropertyBuilderExtensions
    {
        public static DependencyPropertyBuilder<T, TValue> PropertyMetadata<T, TValue>(
            this DependencyPropertyBuilder<T, TValue> builder,
            Action<PropertyMetadataBuilder<T, TValue>> build
        ) where T : DependencyObject
        {
            var metadataBuilder = new PropertyMetadataBuilder<T, TValue>();
            build(metadataBuilder);
            return builder.Metadata(metadataBuilder);
        }
    }
}