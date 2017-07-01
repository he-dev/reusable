using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.Colin.Data
{
    public class CommandParameter
    {
        public CommandParameter([NotNull] PropertyInfo property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Name = ImmutableNameSet.From(property);
            (Required, Position) = property.GetCustomAttribute<ParameterAttribute>();
        }

        public PropertyInfo Property { get; }

        [NotNull]
        public IImmutableNameSet Name { get; }

        [CanBeNull]
        public object DefaultValue => Property.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        public bool Required { get; }

        public int Position { get; }
    }
}
