using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Exceptionize;

namespace Reusable.OneTo1
{
    // This converter diverges from the pure composite pattern and does not use the branch/leaf structure 
    // because with it converters need to be registered in a specific order if they depend on one another.
    public class CompositeConverter : TypeConverter, IEnumerable<ITypeConverter>
    {
        // We cannot use a dictionary because there are not unieque keys. Generic types can have multiple matches.
        private readonly HashSet<ITypeConverter> _converters;

        private readonly ConcurrentDictionary<(Type fromType, Type toType), ITypeConverter> _cache;

        public CompositeConverter()
        {
            _converters = new HashSet<ITypeConverter>();
            _cache = new ConcurrentDictionary<(Type fromType, Type toType), ITypeConverter>();
        }

        internal CompositeConverter(params ITypeConverter[] converters) : this()
        {
            foreach (var (converter, i) in converters.Select((x, i) => (x, i)))
            {
                switch (converter)
                {
                    case null: throw new ArgumentNullException($"Converter at {i} is null.");

                    // Flatten any composite converters for faster access.
                    case IEnumerable<ITypeConverter> composite:
                        _converters.UnionWith(composite);
                        break;

                    default:
                        _converters.Add(converter);
                        break;
                }
            }
        }

        public override Type FromType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(FromType)} property.");

        public override Type ToType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(ToType)} property");

        private bool TryGetConverter(Type fromType, Type toType, out ITypeConverter converter)
        {
            converter = _cache.GetOrAdd((fromType, toType), key => _converters.FirstOrDefault(x => x.CanConvert(fromType, toType)));
            return converter != null;
        }

        protected override bool SupportsConversion(Type fromType, Type toType) => TryGetConverter(fromType, toType, out _);

        protected override object ConvertCore(IConversionContext<object> context)
        {
            if (TryGetConverter(context.FromType, context.ToType, out var converter))
            {
                return converter.Convert(context);
            }
            else
            {
                throw DynamicException.Create
                (
                   $"TypeConverterNotFound{nameof(Exception)}",
                   $"Cannot convert from '{context.FromType.Name}' to '{context.ToType.Name}."
                );
            }
        }

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

    //public static class CompositeConverterExtensions
    //{
    //    public static void Add(this CompositeConverter converter, Type converterType)
    //    {
    //        if (!typeof(ITypeConverter).IsAssignableFrom(converterType))
    //        {
    //            throw new ArgumentException($"'{nameof(converterType)}' must by of type '{nameof(ITypeConverter)}'");
    //        }

    //        _converters.Add((ITypeConverter)Activator.CreateInstance(converterType));
    //    }
    //}
}