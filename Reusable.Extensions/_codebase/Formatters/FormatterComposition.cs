using System.Globalization;

namespace Reusable.Formatters
{
    public static class FormatterComposition
    {
        public static Formatter Add<T>(this Formatter formatter, CultureInfo culture = null)
            where T : Formatter, new()
        {
            return new CompositeFormatter(culture, new T(), formatter);
        }
    }
}