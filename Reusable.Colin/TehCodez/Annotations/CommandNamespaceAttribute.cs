using System;

namespace Reusable.Colin.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNamespaceAttribute : Attribute
    {
        private readonly string _name;

        public CommandNamespaceAttribute(string name) => _name = name ?? throw new ArgumentNullException(nameof(name));

        public bool Required { get; set; }

        public override string ToString() => _name;

        public static implicit operator string(CommandNamespaceAttribute attr) => attr?.ToString();
    }
}