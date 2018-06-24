using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.FormatProviders
{
    public class TypeFormatProvider : CustomFormatProvider
    {
        public TypeFormatProvider()
            : base(new TypeFormatter())
        { }

        private class TypeFormatter : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg is null) { return string.Empty; }

                if (arg is Type type)
                {
                    var typeString = type.ToPrettyString();
                    return
                        format is null
                            ? typeString
                            : string.Format(formatProvider, $"{{0:{format}}}", typeString);
                }

                return null;
            }
        }
    }

    internal class CultureFormatProvider : IFormatProvider
    {
        private readonly CultureInfo _culture;

        public CultureFormatProvider(CultureInfo culture) => _culture = culture ?? throw new ArgumentNullException(nameof(culture));

        public object GetFormat(Type formatType) => _culture.GetFormat(formatType);        
    }
}