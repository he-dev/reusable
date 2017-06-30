using System;

namespace Reusable.SmartConfig.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DatastoreAttribute : Attribute
    {
        private readonly string _name;
        public DatastoreAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(DatastoreAttribute obj) => obj?.ToString();
    }
}