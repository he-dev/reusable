using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections;
using Reusable.Exceptionize;

namespace Reusable.Converters
{
    // This converter diverges from the pure composite pattern and does not use the branch/leaf structure 
    // because with it converters need to be registered in a specific order if they depend on one another.
    public class CompositeConverter : TypeConverter, IEnumerable<ITypeConverter>
    {
        // We cannot use a dictionary because there are not unieque keys. Generic types can have multiple matches.
        private readonly HashSet<ITypeConverter> _converters;

        public CompositeConverter()
        {
            _converters = new HashSet<ITypeConverter>();
        }

        internal CompositeConverter(params ITypeConverter[] converters) : this()
        {
            foreach (var converter in converters)
            {
                switch (converter)
                {
                    // Flatten any composite converters for faster access.
                    case IEnumerable<ITypeConverter> composite:
                        _converters.UnionWith(composite); //.AddRange(composite.Select(c => (Key: (c.FromType, c.ToType), Value: c)));
                        break;
                    default:
                        _converters.Add(converter); // (converter.FromType, converter.ToType), converter);
                        break;
                }
            }

            //_converters = converters;
        }

        public override Type FromType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(FromType)} property.");

        public override Type ToType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(ToType)} property");

        public override bool CanConvert(Type fromType, Type toType)
        {
            return _converters.Any(c => c.CanConvert(fromType, toType));
        }

        protected override object ConvertCore(IConversionContext<object> context)
        {
            if (IsConverted(context.FromType, context.ToType))
            {
                return context.Value;
            }

            var converter = _converters.FirstOrDefault(x => x.CanConvert(context.FromType, context.ToType));

            if (converter == null)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"TypeConverterNotFound{nameof(Exception)}",
                    $"Could not find converter from '{context.FromType.Name}' to '{context.ToType.Name}.",
                    null);
            }

            return converter.Convert(new ConversionContext<object>(context.Value, context.ToType)
            {
                Format = context.Format,
                FormatProvider = context.FormatProvider,
                Converter = this
            });
        }

        public static CompositeConverter operator +(CompositeConverter left, TypeConverter right)
        {
            return new CompositeConverter(left, right);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ITypeConverter> GetEnumerator()
        {
            return _converters.GetEnumerator();
        }
    }
}