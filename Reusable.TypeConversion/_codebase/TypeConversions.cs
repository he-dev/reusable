using System;
using System.Globalization;

namespace Reusable
{
    public static class TypeConversions
    {
        public static object Convert(this TypeConverter converter, object arg, Type type, CultureInfo culture = null)
        {
            // conversion not necessary
            if (arg.GetType() == type)
            {
                return arg;
            }

            var instance = (object)null;
            if (!converter.TryConvert(arg, type, culture ?? CultureInfo.InvariantCulture, out instance))
            {
                throw new NotSupportedException($"Type '{type.FullName}' is not supported.");
            }

            return instance;
        }

        public static object Convert<T>(this TypeConverter converter, object arg, CultureInfo culture = null)
            => Convert(converter, arg, typeof(T), culture);


        // base method to be used above
        public static bool TryConvert(this TypeConverter converter, object arg, Type type, CultureInfo culture, out object instance) =>
            converter.TryConvert(new ConversionContext(converter, type, culture), arg, out instance);
    }
}
