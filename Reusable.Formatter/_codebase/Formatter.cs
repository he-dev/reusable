using System;
using System.Globalization;

namespace Reusable
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

        public CompositeFormatter(params Formatter[] formatters)
        : this((CultureInfo)null, formatters)
        { }

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            foreach (var formatter in _formatters)
            {
                var result = formatter.Format(format, arg, formatProvider);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
