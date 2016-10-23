using System.Globalization;

namespace Reusable
{
    public static class FormatterComposer
    {
        public static Formatter Add<T>(this Formatter formatter, CultureInfo culture = null)
            where T : Formatter, new()
        {
            return new CompositeFormatter(culture, new T(), formatter);
        }
    }
}