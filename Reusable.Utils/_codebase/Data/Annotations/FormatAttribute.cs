using System;

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

        public IFormatProvider FormatProvider { get; set; }

        public string FormatString { get; }

        public override string ToString()
        {
            return FormatString;
        }
    }
}
