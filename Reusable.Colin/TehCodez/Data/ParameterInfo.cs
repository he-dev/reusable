using Reusable.TypeConversion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            (Required, Position, ListSeparator) = property.GetCustomAttribute<ParameterAttribute>();
        }

        public PropertyInfo Property { get; }

        public ImmutableNameSet Names { get; }

        public bool Required { get; }

        public int Position { get; }

        public char ListSeparator { get; }
    }
}
