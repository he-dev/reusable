﻿using System;

namespace Reusable.Converters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute(Type type)
        {
            if (!typeof(TypeConverter).IsAssignableFrom(type))
            {
                throw new ArgumentException($"'{nameof(type)}' must implement the '{typeof(TypeConverter).FullName}'", nameof(type));
            }
            Type = type;
        }

        public Type Type { get; }
    }
}
