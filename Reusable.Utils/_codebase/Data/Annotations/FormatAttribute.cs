using System;
using System.Globalization;
// ReSharper disable MemberCanBePrivate.Global

namespace Reusable.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FormatAttribute : Attribute
    {
        private FormatAttribute(string formatString, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(formatString)) throw new ArgumentException("Value cannot be null or empty.", nameof(formatString));

            FormatString = formatString;
            FormatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
        }

        public FormatAttribute(string formatString, Type formatProviderType)
            : this(formatString, (IFormatProvider)Activator.CreateInstance(formatProviderType))
        { }

        public FormatAttribute(string formatString)
            : this(formatString, CultureInfo.InvariantCulture)
        { }

        public IFormatProvider FormatProvider { get; set; }

        public string FormatString { get; }

        public override string ToString()
        {
            return FormatString;
        }
    }
}
