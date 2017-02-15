using System;
using System.Globalization;

namespace Reusable.Converters
{
    public static class TypeConversions
    {
        //public static object Convert(this TypeConverter converter, object value, Type type, CultureInfo culture = null)
        //{
        //    // conversion not necessary
        //    if (value.GetType() == type)
        //    {
        //        return value;
        //    }

        //    var instance = (object)null;
        //    if (!converter.TryConvert(value, type, culture ?? CultureInfo.InvariantCulture, out instance))
        //    {
        //        throw new NotSupportedException($"Type '{type.FullName}' is not supported.");
        //    }

        //    return instance;
        //}

        //public static object Convert<T>(this TypeConverter converter, object value, CultureInfo culture = null)
        //    => Convert(converter, value, typeof(T), culture);


        //// base method to be used above
        //public static bool TryConvert(this TypeConverter converter, object value, Type targetType, CultureInfo culture, out object instance) =>
        //    converter.TryConvert(new ConversionContext(value, targetType, converter, culture), out instance);
    }
}
