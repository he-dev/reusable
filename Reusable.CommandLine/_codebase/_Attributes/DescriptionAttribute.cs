using System;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DescriptionAttribute : Attribute
    {
        private readonly string _text;

        public DescriptionAttribute(string helpText)
        {
            _text = helpText;
        }

        public override string ToString()
        {
            return _text;
        }

        public static implicit operator string(DescriptionAttribute descriptionAttribute)
        {
            return descriptionAttribute.ToString();
        }
    }
}