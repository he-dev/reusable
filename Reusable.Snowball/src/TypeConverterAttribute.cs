using System;
using JetBrains.Annotations;

namespace Reusable.Snowball;

[UsedImplicitly]
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TypeConverterAttribute : Attribute
{
    public TypeConverterAttribute(Type converterType)
    {
        if (!typeof(ITypeConverter).IsAssignableFrom(converterType))
        {
            throw new ArgumentException($"'{nameof(converterType)}' must implement the '{typeof(ITypeConverter).FullName}'", nameof(converterType));
        }
        ConverterType = converterType;
    }
        
    public Type ConverterType { get; }
}