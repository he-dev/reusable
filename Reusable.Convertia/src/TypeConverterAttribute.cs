using System;

namespace Reusable.Convertia
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute(Type converterType)
        {
            if (!typeof(TypeConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException($"'{nameof(converterType)}' must implement the '{typeof(TypeConverter).FullName}'", nameof(converterType));
            }
            ConverterType = converterType;
        }

        public Type ConverterType { get; }
    }
}
