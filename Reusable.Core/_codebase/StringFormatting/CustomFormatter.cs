using System;
using System.Globalization;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.StringFormatting
{
    public abstract class CustomFormatter : IFormatProvider, ICustomFormatter
    {
        protected CustomFormatter(CultureInfo culture = null)
        {
            Culture = culture ?? CultureInfo.InvariantCulture;
        }

        public static CustomFormatter Default() => new DefaultFormatter();

        public CultureInfo Culture { get; }

        public virtual object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) || formatType == GetType() ? this : null;
        }

        public abstract string Format(string format, object arg, IFormatProvider formatProvider);
    }

    public class DefaultFormatter : CustomFormatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return null;
        }
    }

    public class CompositeFormatter : CustomFormatter
    {
        private readonly CustomFormatter[] _formatters;

        public CompositeFormatter(CultureInfo culture, params CustomFormatter[] formatters) : base(culture)
        {
            _formatters = formatters;
        }

        public CompositeFormatter(params CustomFormatter[] formatters) : this(null, formatters)
        { }

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return 
                _formatters
                    .Select(formatter => formatter.Format(format, arg, formatProvider))
                    .FirstOrDefault(Conditional.IsNotNull);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class FormatProviderAttribute : Attribute
    {
        public FormatProviderAttribute(Type type)
        {
            if (!typeof(IFormatProvider).IsAssignableFrom(type))
            {
                throw new ArgumentException($"'{nameof(type)}' must implement the '{typeof(IFormatProvider).FullName}'.", nameof(type));
            }
            FormatProvider = (IFormatProvider)Activator.CreateInstance(type);
        }

        public string Format { get; set; }

        public IFormatProvider FormatProvider { get; }
    }
}
