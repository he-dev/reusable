using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute(Type type)
        {
            if (!typeof(TypeConverter).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type must be derived from '{typeof(TypeConverter).FullName}'", nameof(type));
            }
            Type = type;
        }

        public Type Type { get; }
    }
}
