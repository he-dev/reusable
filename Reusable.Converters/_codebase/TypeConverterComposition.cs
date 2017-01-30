using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Fuse;

namespace Reusable.Converters
{
    public static class TypeConverterComposition
    {
        public static TypeConverter Add(this TypeConverter current, TypeConverter register)
        {
            current.Validate(nameof(current)).IsNotNull();
            register.Validate(nameof(register)).IsNotNull();

            return new CompositeConverter(current, register);
        }

        public static TypeConverter Add(this TypeConverter current, Type converterType)
        {
            current.Validate(nameof(current)).IsNotNull();
            converterType.Validate(nameof(converterType)).IsNotNull();

            return current.Add((TypeConverter)Activator.CreateInstance(converterType));
        }

        public static TypeConverter Add<TConverter>(this TypeConverter current) where TConverter : TypeConverter, new()
        {
            current.Validate(nameof(current)).IsNotNull();

            return current.Add(new TConverter());
        }

        public static TypeConverter Add(this TypeConverter current, params TypeConverter[] converters)
        {
            return current.Add((IEnumerable<TypeConverter>)converters);
        }

        public static TypeConverter Add(this TypeConverter current, IEnumerable<TypeConverter> converters)
        {
            current.Validate(nameof(current)).IsNotNull();
            converters.Validate(nameof(converters)).IsNotNull();

            return converters.Aggregate(current, (x, converter) => x.Add(converter));
        }

        public static TypeConverter Remove(this TypeConverter current, IEnumerable<TypeConverter> remove)
        {
            var compositeConverter = current as CompositeConverter;
            if (compositeConverter == null) return new CompositeConverter(current);
            var converters = compositeConverter.Except(remove);
            return new CompositeConverter(converters);
        }

        public static TypeConverter Remove(this TypeConverter current, params TypeConverter[] remove)
        {
            return current.Remove((IEnumerable<TypeConverter>) remove);
        }
    }
}
