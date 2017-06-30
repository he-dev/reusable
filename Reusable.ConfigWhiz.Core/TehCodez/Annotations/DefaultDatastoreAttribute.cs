using System;

namespace Reusable.SmartConfig.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DefaultDatastoreAttribute : Attribute
    {
        private readonly string _name;
        public DefaultDatastoreAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(DefaultDatastoreAttribute obj) => obj?.ToString();
    }
}