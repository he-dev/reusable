using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.Shelly
{
    internal class CommandProperty
    {
        private readonly PropertyInfo _property;

        public CommandProperty(PropertyInfo property)
        {
            _property = property;
        }

        public IEnumerable<string> Names
        {
            get
            {
                yield return _property.Name;

                var alias = _property.GetCustomAttribute<ShortcutAttribute>();
                foreach (var a in alias ?? Enumerable.Empty<string>())
                {
                    yield return a;
                }
            }
        }

        public Type Type => _property.PropertyType;

        public bool Mandatory => _property.GetCustomAttribute<ParameterAttribute>().Mandatory;

        public int Position => _property.GetCustomAttribute<ParameterAttribute>().Position;

        public char ListSeparator => _property.GetCustomAttribute<ParameterAttribute>().ListSeparator;

        public void SetValue(object obj, object value)
        {
            _property.SetValue(obj, value);
        }
    }
}