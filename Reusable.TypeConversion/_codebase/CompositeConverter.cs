using System.Linq;

namespace Reusable
{
    public class CompositeConverter : TypeConverter
    {
        internal CompositeConverter() { }

        internal CompositeConverter(TypeConverter first, TypeConverter second)
        {
            var currentConverters = (first as CompositeConverter)?.Converters ?? new[] { first };
            Converters = currentConverters.Concat(new[] { second }).ToArray();
        }        

        //public static readonly TypeConverter Empty = new CompositeConverter();

        public TypeConverter[] Converters { get; } = Enumerable.Empty<TypeConverter>().ToArray();

        public CompositeConverter Add<TConverter>() where TConverter : TypeConverter, new() => (this + new TConverter());

        //public CompositeConverter Add() where TConverter : TypeConverter, new() => (this + new TConverter());

        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            foreach (var converter in Converters)
            {
                if (converter.TryConvert(context, arg, out instance))
                {
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public static CompositeConverter operator +(CompositeConverter left, TypeConverter right) =>
            new CompositeConverter(left, right);
    }
}
