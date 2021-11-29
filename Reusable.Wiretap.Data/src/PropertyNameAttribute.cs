using System;

namespace Reusable.Wiretap.Data
{
    public class PropertyNameAttribute : Attribute
    {
        private readonly string _name;
        public PropertyNameAttribute(string name) => _name = name;
        public override string ToString() => _name;
    }
}