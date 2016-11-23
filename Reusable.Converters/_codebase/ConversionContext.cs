using System;
using System.Globalization;

namespace Reusable.Converters
{
    public class ConversionContext
    {
        public ConversionContext(TypeConverter service, Type type, CultureInfo culture)
        {
            Service = service;
            Type = type;
            Culture = culture;
        }

        public TypeConverter Service { get; }
        public Type Type { get; }
        public CultureInfo Culture { get; }
    }
}
