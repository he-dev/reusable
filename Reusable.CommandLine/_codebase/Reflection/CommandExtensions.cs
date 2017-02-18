using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Fuse;

namespace Reusable.Shelly.Reflection
{
    internal static class CommandExtensions
    {
        public static IEnumerable<string> GetPropertyNames(this PropertyInfo property)
        {
            property.Validate(nameof(property)).IsNotNull();

            var customNames = property.GetCustomAttribute<ShortcutsAttribute>();
            if (customNames != null)
            {
                return customNames;
            }

            return new[] { property.Name };
        }

        public static string GetDescription(this Type type)
        {
            return type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }
}
