using System;
using JetBrains.Annotations;

namespace Reusable.OneTo1
{
    [UsedImplicitly]
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute([NotNull] Type converterType)
        {
            if (converterType == null) throw new ArgumentNullException(nameof(converterType));
            if (!typeof(ITypeConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException($"'{nameof(converterType)}' must implement the '{typeof(ITypeConverter).FullName}'", nameof(converterType));
            }
            ConverterType = converterType;
        }

        [NotNull]
        public Type ConverterType { get; }
    }
}
