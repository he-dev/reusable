using System;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NamespaceAttribute : Attribute
    {
        private readonly string _name;

        public NamespaceAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _name = name;
        }

        public bool Mandatory { get; set; }

        public override string ToString() => _name;

        public static implicit operator string(NamespaceAttribute namespaceAttribute) => namespaceAttribute.ToString();
    }
}