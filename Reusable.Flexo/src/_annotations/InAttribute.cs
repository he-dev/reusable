using System;

namespace Reusable.Flexo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InAttribute : Attribute, IParameterAttribute
    {
        public InAttribute(string name) => Name = name;

        public string Name { get; }

        public bool Required { get; set; } = true;
    }
}