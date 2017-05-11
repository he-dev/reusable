using System;

namespace Reusable.Data.SqlClient
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SchemaColumnNameAttribute : Attribute
    {
        private readonly string _name;
        public SchemaColumnNameAttribute(string name) => _name = name;
        public override string ToString() => _name;
        public static implicit operator string(SchemaColumnNameAttribute attribute) => attribute.ToString();
    }
}