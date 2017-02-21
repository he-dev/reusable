using Reusable.Shelly.Collections;
using Reusable.TypeConversion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Data
{
    internal class ParameterInfo
    {
        private readonly PropertyInfo _propertyInfo;

        public ParameterInfo(PropertyInfo property)
        {
            Property = property;
            Names = GetNames(property);

            var parameterAtrribute = property.GetCustomAttribute<ParameterAttribute>();
            Required = parameterAtrribute.Required;
            Position = parameterAtrribute.Position;
            ListSeparator = parameterAtrribute.ListSeparator;
        }

        public PropertyInfo Property { get; }

        public ImmutableHashSet<string> Names { get; }

        public bool Required { get; }

        public int? Position { get; }

        public char? ListSeparator { get; }

        public static ImmutableHashSet<string> GetNames(PropertyInfo property)
        {
            var names = new List<string>();

            var parameterAtrribute = property.GetCustomAttribute<ParameterAttribute>();
            if (string.IsNullOrEmpty(parameterAtrribute.Name))
            {
                names.Add(property.Name);
            }
            else
            {
                names.Add(parameterAtrribute.Name);
            }

            names.AddRange(property.GetCustomAttribute<ShortcutsAttribute>() ?? Enumerable.Empty<string>());
            return ImmutableNameSet.Create(names);
        }
    }
}
