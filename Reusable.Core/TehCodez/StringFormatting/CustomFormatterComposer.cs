using System.Globalization;

namespace Reusable.StringFormatting
{
    public static class CustomFormatterComposer
    {
        public static CustomFormatter Add<T>(this CustomFormatter formatter, CultureInfo culture)
            where T : CustomFormatter, new()
        {
            return new CompositeFormatter(culture, new T(), formatter);
        }

        public static CustomFormatter Add<T>(this CustomFormatter formatter)
            where T : CustomFormatter, new()
        {
            return formatter.Add<T>(CultureInfo.InvariantCulture);
        }
    }
}