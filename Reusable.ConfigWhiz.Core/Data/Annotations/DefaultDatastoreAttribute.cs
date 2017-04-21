using System;

namespace Reusable.ConfigWhiz.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultDatastoreAttribute : Attribute
    {
        private readonly string _name;
        public DefaultDatastoreAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(DefaultDatastoreAttribute obj) => obj?.ToString();
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class FallbackDatastoreAttribute : Attribute
    {
        private readonly string _name;
        public FallbackDatastoreAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(FallbackDatastoreAttribute obj) => obj?.ToString();
    }
}