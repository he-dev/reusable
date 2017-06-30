using System;

namespace Reusable.SmartConfig.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class FallbackDatastoreAttribute : Attribute
    {
        private readonly string _name;
        public FallbackDatastoreAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(FallbackDatastoreAttribute obj) => obj?.ToString();
    }
}