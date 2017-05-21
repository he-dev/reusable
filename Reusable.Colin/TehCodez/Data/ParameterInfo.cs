using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    internal class ParameterInfo
    {
        public ParameterInfo(PropertyInfo property)
        {
            Property = property;
            Names = ImmutableNameSet.From(property);
            (Required, Position) = property.GetCustomAttribute<ParameterAttribute>();
        }

        public PropertyInfo Property { get; }

        [CanBeNull]
        public object DefaultValue => Property.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        public ImmutableNameSet Names { get; }

        public bool Required { get; }

        public int Position { get; }
    }
}
