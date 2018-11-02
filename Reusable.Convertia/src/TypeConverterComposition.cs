using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Convertia
{
    public static class TypeConverterComposition
    {
        public static ITypeConverter Add(this ITypeConverter current, ITypeConverter register)
        {
            return new CompositeConverter(
                current ?? throw new ArgumentNullException(nameof(current)),
                register ?? throw new ArgumentNullException(nameof(register))
            );
        }

        public static ITypeConverter Add(this ITypeConverter current, Type converterType)
        {
            return current.Add((TypeConverter)Activator.CreateInstance(converterType ?? throw new ArgumentNullException(nameof(converterType))));
        }

        public static ITypeConverter Add<TConverter>(this ITypeConverter current) where TConverter : ITypeConverter, new()
        {
            return (current ?? throw new ArgumentNullException(nameof(current))).Add(new TConverter());
        }

        public static ITypeConverter Add(this ITypeConverter current, params ITypeConverter[] converters)
        {
            return current.Add((IEnumerable<ITypeConverter>)converters);
        }

        public static ITypeConverter Add(this ITypeConverter current, IEnumerable<ITypeConverter> converters)
        {
            return (converters ?? throw new ArgumentNullException(nameof(converters))).Aggregate(
                (current ?? throw new ArgumentNullException(nameof(current))),
                (x, converter) => x.Add(converter));
        }

        public static ITypeConverter Remove(this ITypeConverter current, IEnumerable<ITypeConverter> remove)
        {
            if (!(current is CompositeConverter compositeConverter)) return new CompositeConverter(current);
            var converters = compositeConverter.Except(remove).ToArray();
            return new CompositeConverter(converters);
        }

        public static ITypeConverter Remove(this ITypeConverter current, params TypeConverter[] remove)
        {
            return current.Remove((IEnumerable<ITypeConverter>)remove);
        }
    }
}
