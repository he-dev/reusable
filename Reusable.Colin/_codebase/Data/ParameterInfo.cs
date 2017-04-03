using Reusable.TypeConversion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    internal class ParameterInfo
    {
        public ParameterInfo(PropertyInfo property)
        {
            Property = property;
            Names = ImmutableNameSet.Create(GetNames(property));
            (Required, Position, ListSeparator) = property.GetCustomAttribute<ParameterAttribute>();
        }

        public PropertyInfo Property { get; }

        public ImmutableNameSet Names { get; }

        public bool Required { get; }

        public int Position { get; }

        public char ListSeparator { get; }

        public static IEnumerable<string> GetNames(PropertyInfo property)
        {
            var customName = property.GetCustomAttribute<ParameterAttribute>()?.Name;
            yield return 
                string.IsNullOrEmpty(customName)
                    ? property.Name
                    : customName;

            foreach (var name in property.GetCustomAttribute<AlsoKnownAsAttribute>() ?? Enumerable.Empty<string>())
            {
                yield return name;
            }
        }
    }
}
