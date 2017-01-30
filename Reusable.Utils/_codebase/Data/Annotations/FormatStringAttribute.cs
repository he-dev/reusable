using System;

namespace Reusable.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FormatStringAttribute : Attribute
    {
        public FormatStringAttribute(string formatString)
        {
            FormatString = formatString;
        }

        public string FormatString { get; }

        public override string ToString()
        {
            return FormatString;
        }
    }
}
