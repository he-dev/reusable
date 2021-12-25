using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Snowball;

[PublicAPI]
public static class TypeConverterComposition
{
    [DebuggerStepThrough]
    public static ITypeConverter Push(this ITypeConverter current, params ITypeConverter[] converters)
    {
        return new TypeConverterStack(converters.Prepend(current).Select(converter => converter.Decorate()));
    }

    [DebuggerStepThrough]
    public static ITypeConverter Push<TConverter>(this ITypeConverter current) where TConverter : ITypeConverter, new()
    {
        return current.Push(typeof(TConverter));
    }

    [DebuggerStepThrough]
    public static ITypeConverter Push(this ITypeConverter current, Type type)
    {
        var converter = Activator.CreateInstance(type) as ITypeConverter ?? throw new ArgumentException($"{nameof(type)} must be {nameof(ITypeConverter)} but was {type.ToPrettyString()}");
        return current.Push(converter);
    }
}