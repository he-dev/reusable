using System;
using System.Globalization;

namespace Reusable.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FormatAttribute : Attribute
    {
        public FormatAttribute(Type formatProviderType, string formatString)
        {
            FormatProvider = (IFormatProvider)Activator.CreateInstance(formatProviderType);
            FormatString = formatString;
        }

        public FormatAttribute(string formatString)
        {
            FormatProvider = CultureInfo.InvariantCulture; // (IFormatProvider)Activator.CreateInstance(formatProviderType);
            FormatString = formatString;
        }

        public IFormatProvider FormatProvider { get; set; }

        public string FormatString { get; }

        public override string ToString()
        {
            return FormatString;
        }
    }
}
