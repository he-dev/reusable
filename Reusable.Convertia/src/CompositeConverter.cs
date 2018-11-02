using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Convertia
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
            foreach (var converter in converters)
            {
                switch (converter)
                {
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

        // Finds a converter and caches the conversion.
        [CanBeNull]
        private ITypeConverter this[(Type fromType, Type toType) index] => _cache.GetOrAdd(index, key => _converters.FirstOrDefault(x => x.CanConvert(key.fromType, key.toType)));

        public override bool CanConvert(Type fromType, Type toType) => !(this[(fromType, toType)] is null);

        protected override object ConvertCore(IConversionContext<object> context)
        {
            var converter =
                this[(context.FromType, context.ToType)]
                ?? throw DynamicException.Factory.CreateDynamicException(
                    $"TypeConverterNotFound{nameof(Exception)}",
                    $"Cannot convert from '{context.FromType.Name}' to '{context.ToType.Name}.",
                    null);

            return converter.Convert(context);
        }

        public void Add(Type converterType)
        {
            if (!typeof(ITypeConverter).IsAssignableFrom(converterType))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"InvalidType{nameof(Exception)}",
                    $"'{nameof(converterType)}' must by of type '{nameof(ITypeConverter)}'");
            }

            _converters.Add((ITypeConverter)Activator.CreateInstance(converterType));
        }

        public static CompositeConverter operator +(CompositeConverter left, TypeConverter right) => new CompositeConverter(left, right);

        public IEnumerator<ITypeConverter> GetEnumerator() => _converters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}