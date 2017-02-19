using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.Shelly
{
    internal static class CommandParameter
    {
        public static StringSet GetNames(PropertyInfo property)
        {
            var names = new List<string>();

            var parameterAtrribute = property.GetCustomAttribute<ParameterAttribute>();
            if (!string.IsNullOrEmpty(parameterAtrribute.Name)) names.Add(parameterAtrribute.Name);
            else names.Add(property.Name);

            names.AddRange(property.GetCustomAttribute<ShortcutsAttribute>() ?? Enumerable.Empty<string>());
            return StringSet.CreateCI(names);
        }
    }

    //public Type Type => _property.PropertyType;

    //public bool Mandatory => _property.GetCustomAttribute<ParameterAttribute>().Required;

    //public int Position => _property.GetCustomAttribute<ParameterAttribute>().Position;

    //public char ListSeparator => _property.GetCustomAttribute<ParameterAttribute>().ListSeparator;

    //public void SetValue(object obj, object value)
    //{
    //    _property.SetValue(obj, value);
    //}

}