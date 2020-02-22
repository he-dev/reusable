using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
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
            return new TypeConverterStack(converters.Prepend(current).Select(converter => DecoratorScope<ITypeConverter>.Current?.Decorate(converter) ?? converter));
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
}