using System;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OutParameterAttribute : Attribute, IParameterAttribute
    {
        public OutParameterAttribute(string name) => Name = name;

        public string Name { get; }

        public bool Required { get; set; } = true;
    }
}