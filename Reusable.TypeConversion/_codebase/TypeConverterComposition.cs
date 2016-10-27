using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public static class TypeConverterComposition
    {
        public static TypeConverter Add<TConverter>(this TypeConverter current)
            where TConverter : TypeConverter, new() 
            => current.Add(new TConverter());

        public static TypeConverter Add(this TypeConverter current, Type converterType)
            => current.Add((TypeConverter)Activator.CreateInstance(converterType));

        public static TypeConverter Add(this TypeConverter current, IEnumerable<TypeConverter> converters) 
            => converters.Aggregate(current, (x, converter) => x.Add(converter));

        // etc…

        // base method to be used above
        public static TypeConverter Add(this TypeConverter current, TypeConverter register) 
            => new CompositeConverter(current, register);
    }
}
