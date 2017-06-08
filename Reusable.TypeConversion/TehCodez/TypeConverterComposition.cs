using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.TypeConversion
{
    public static class TypeConverterComposition
    {
        public static TypeConverter Add(this TypeConverter current, TypeConverter register)
        {
            return new CompositeConverter(
                current ?? throw new ArgumentNullException(nameof(current)),
                register ?? throw new ArgumentNullException(nameof(register))
            );
        }

        public static TypeConverter Add(this TypeConverter current, Type converterType)
        {
            return current.Add((TypeConverter)Activator.CreateInstance(converterType ?? throw new ArgumentNullException(nameof(converterType))));
        }

        public static TypeConverter Add<TConverter>(this TypeConverter current) where TConverter : TypeConverter, new()
        {
            return (current ?? throw new ArgumentNullException(nameof(current))).Add(new TConverter());
        }

        public static TypeConverter Add(this TypeConverter current, params TypeConverter[] converters)
        {
            return current.Add((IEnumerable<TypeConverter>)converters);
        }

        public static TypeConverter Add(this TypeConverter current, IEnumerable<TypeConverter> converters)
        {
            return (converters ?? throw new ArgumentNullException(nameof(converters))).Aggregate(
                (current ?? throw new ArgumentNullException(nameof(current))),
                (x, converter) => x.Add(converter));
        }

        public static TypeConverter Remove(this TypeConverter current, IEnumerable<TypeConverter> remove)
        {
            var compositeConverter = current as CompositeConverter;
            if (compositeConverter == null) return new CompositeConverter(current);
            var converters = compositeConverter.Except(remove).ToArray();
            return new CompositeConverter(converters);
        }

        public static TypeConverter Remove(this TypeConverter current, params TypeConverter[] remove)
        {
            return current.Remove((IEnumerable<TypeConverter>)remove);
        }
    }
}
