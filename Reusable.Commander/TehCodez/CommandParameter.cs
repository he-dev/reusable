using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Reusable.Commander
{
    public class CommandParameter
    {
        private readonly PropertyInfo _property;

        private CommandParameter(PropertyInfo property)
        {
            _property = property;
            Name = NameFactory.CreatePropertyName(property);
            Type = _property.PropertyType;
            Metadata = _property.GetCustomAttribute<ParameterAttribute>();
        }

        public SoftKeySet Name { get; }

        public Type Type { get; }

        public ParameterAttribute Metadata { get; }

        public object DefaultValue => _property.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        public bool IsRequired => _property.IsDefined(typeof(RequiredAttribute)) || Metadata.Position > CommandLine.CommandIndex;

        public static CommandParameter Create(PropertyInfo property) => new CommandParameter(property);

        public void SetValue(object obj, object value) => _property.SetValue(obj, value);
    }
}