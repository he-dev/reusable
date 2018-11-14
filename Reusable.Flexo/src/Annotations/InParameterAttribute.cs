using System;
using Reusable.Flexo.Expressions;

namespace Reusable.Flexo.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InParameterAttribute : Attribute, IParameterAttribute
    {
        public InParameterAttribute(string name) => Name = name;

        public string Name { get; }
        
        public bool Required { get; set; } = true;
    }
}