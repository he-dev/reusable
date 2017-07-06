using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Data
{
    public class ArgumentMetadata
    {
        public ArgumentMetadata(IImmutableNameSet name, [NotNull] PropertyInfo property)
        {
            Name = name;
            Property = property ?? throw new ArgumentNullException(nameof(property));
            (Required, Position) = property.GetCustomAttribute<ParameterAttribute>();
        }

        [NotNull]
        public IImmutableNameSet Name { get; }

        [NotNull]
        public PropertyInfo Property { get; }

        [CanBeNull]
        public object DefaultValue => Property.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        public bool Required { get; }

        public int Position { get; }
    }
}
