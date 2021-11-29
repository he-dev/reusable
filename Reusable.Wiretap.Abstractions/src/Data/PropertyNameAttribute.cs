using System;

namespace Reusable.OmniLog.Abstractions
{
    public class PropertyNameAttribute : Attribute
    {
        private readonly string _name;
        public PropertyNameAttribute(string name) => _name = name;
        public override string ToString() => _name;
    }
}