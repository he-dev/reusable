using System;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NamespaceAttribute : Attribute
    {
        private readonly string _name;

        public NamespaceAttribute(string name) => _name = name ?? throw new ArgumentNullException(nameof(name));

        public bool Mandatory { get; set; }

        public override string ToString() => _name;

        public static implicit operator string(NamespaceAttribute namespaceAttribute) => namespaceAttribute?.ToString();
    }
}