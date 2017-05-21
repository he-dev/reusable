using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    public class CommandParameter
    {
        public CommandParameter(PropertyInfo property)
        {
            Property = property;
            Name = ImmutableNameSet.From(property);
            (Required, Position) = property.GetCustomAttribute<ParameterAttribute>();
        }

        public PropertyInfo Property { get; }

        [CanBeNull]
        public object DefaultValue => Property.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        public ImmutableNameSet Name { get; }

        public bool Required { get; }

        public int Position { get; }
    }
}
