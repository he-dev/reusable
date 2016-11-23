using System;
using System.Globalization;
using System.Linq;

namespace Reusable.Formatters
{
    public abstract class Formatter : IFormatProvider, ICustomFormatter
    {
        protected Formatter(CultureInfo culture = null)
        {
            Culture = culture ?? CultureInfo.InvariantCulture;
        }

        public static Formatter Default(CultureInfo culture = null) => new CompositeFormatter(culture);

        public CultureInfo Culture { get; }

        public virtual object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
        public abstract string Format(string format, object arg, IFormatProvider formatProvider);
    }

    public class CompositeFormatter : Formatter
    {
        private readonly Formatter[] _formatters;

        public CompositeFormatter(CultureInfo culture, params Formatter[] formatters) : base(culture)
        {
            _formatters = formatters;
        }

        public CompositeFormatter(params Formatter[] formatters) : this(null, formatters)
        { }

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return _formatters
                .Select(formatter => formatter.Format(format, arg, formatProvider))
                .FirstOrDefault(result => result != null);
        }
    }
}
