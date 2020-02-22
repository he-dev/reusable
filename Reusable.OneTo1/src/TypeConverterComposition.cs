using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    [PublicAPI]
    public static class TypeConverterComposition
    {
        [DebuggerStepThrough]
        public static ITypeConverter Push(this ITypeConverter current, params ITypeConverter[] converters)
        {
            return new TypeConverterStack(current, new TypeConverterStack(converters));
        }

        [DebuggerStepThrough]
        public static ITypeConverter Push<TConverter>(this ITypeConverter current) where TConverter : ITypeConverter, new()
        {
            return current.Push(new TConverter());
        }

        [DebuggerStepThrough]
        public static ITypeConverter Push(this ITypeConverter current, Type type)
        {
            return current.Push(Activator.CreateInstance(type) as ITypeConverter ?? throw new ArgumentException($"{nameof(type)} must be {nameof(ITypeConverter)} but was {type.ToPrettyString()}"));
        }
    }
}