using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Snowball;

[PublicAPI]
public static class TypeConverterExtensions
{
    [DebuggerStepThrough]
    public static object? ConvertOrDefault(this ITypeConverter converter, object value, Type toType)
    {
        return converter.ConvertOrDefault(value, toType, new ConversionContext
        {
            Converter = converter
        });
    }

    [DebuggerStepThrough]
    public static T ConvertOrDefault<T>(this ITypeConverter converter, object value)
    {
        return ConvertOrDefault(converter, value, typeof(T)) is T result ? result : default;
    }

    [DebuggerStepThrough]
    public static object ConvertOrThrow(this ITypeConverter converter, object value, Type toType)
    {
        return
            converter.ConvertOrDefault(value, toType, new ConversionContext { Converter = converter }) is {} result
                ? result
                : throw DynamicException.Create
                (
                    $"NotSupportedConversion",
                    $"There is no converter from '{value.GetType().ToPrettyString()}' to '{toType.ToPrettyString()}'."
                );
    }

    [DebuggerStepThrough]
    public static T ConvertOrThrow<T>(this ITypeConverter converter, object value)
    {
        return (T)ConvertOrThrow(converter, value, typeof(T));
    }
}