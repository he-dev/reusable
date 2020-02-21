using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.OneTo1
{
    // This converter diverges from the pure composite pattern and does not use the branch/leaf structure 
    // because with it converters need to be registered in a specific order if they depend on one another.
    public class CompositeConverter : TypeConverter, IEnumerable<ITypeConverter>
    {
        // We cannot use a dictionary because there are no unique keys. Generic types can have multiple matches.
        private readonly HashSet<ITypeConverter> _converters;

        private readonly ConcurrentDictionary<(Type fromType, Type toType), ITypeConverter> _cache;

        public CompositeConverter()
        {
            _converters = new HashSet<ITypeConverter>();
            _cache = new ConcurrentDictionary<(Type fromType, Type toType), ITypeConverter>();
        }

        internal CompositeConverter(params ITypeConverter[] converters) : this()
        {
            var query =
                from c in converters
                from x in c as IEnumerable<ITypeConverter> ?? new[] { c }
                where x is {}
                select c;

            _converters.UnionWith(query);
        }

        public override bool CanConvert(Type fromType, Type toType)
        {
            return FindConverter(fromType, toType) is {} converter && converter.CanConvert(fromType, toType);
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            return
                FindConverter(value.GetType(), toType) is {} converter
                    ? converter.Convert(value, toType, context ?? new ConversionContext { Converter = this })
                    : throw NotSupportedConversion(value.GetType(), toType);
        }

        private ITypeConverter? FindConverter(Type fromType, Type toType)
        {
            return
                _cache.TryGetValue((fromType, toType), out var cached)
                    ? cached
                    : _converters.FirstOrDefault(x => x.CanConvert(fromType, toType)) is {} converter
                        ? _cache.AddOrUpdate((fromType, toType), converter, (key, current) => converter)
                        : default;
        }

        public void Add(ITypeConverter converter) => _converters.Add(converter);

        public void Add(Type converterType)
        {
            if (!typeof(ITypeConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException($"'{nameof(converterType)}' must by of type '{nameof(ITypeConverter)}'");
            }

            _converters.Add((ITypeConverter)Activator.CreateInstance(converterType));
        }

        public static CompositeConverter operator +(CompositeConverter left, TypeConverter right) => new CompositeConverter(left, right);

        public IEnumerator<ITypeConverter> GetEnumerator() => _converters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}