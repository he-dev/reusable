using System;
using System.Globalization;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.FormatProviders;

public class TypeFormatProvider : CustomFormatProvider
{
    public TypeFormatProvider()
        : base(new TypeFormatter())
    {
    }

    public static TypeFormatProvider Default { get; } = new();

    private class TypeFormatter : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            switch (arg)
            {
                case null: return string.Empty;

                case Type type:

                    var typeString = type.ToPrettyString(includeNamespace: SoftString.Comparer.Equals(format, "wns"));
                    return
                        format is null
                            ? typeString
                            : string.Format(formatProvider, $"{{0:{format}}}", typeString);

                default: return null;
            }
        }
    }
}

internal class CultureFormatProvider : IFormatProvider
{
    private readonly CultureInfo _culture;

    public CultureFormatProvider(CultureInfo culture) => _culture = culture ?? throw new ArgumentNullException(nameof(culture));

    public object GetFormat(Type formatType) => _culture.GetFormat(formatType);
}