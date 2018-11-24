using System;

namespace Reusable.Flexo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OutAttribute : Attribute, IParameterAttribute
    {
        public OutAttribute(string name) => Name = name;

        public string Name { get; }
        
        public bool Required { get; set; } = true;
    }
}