using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Commander
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandParameter
    {
        private readonly PropertyInfo _property;

        private CommandParameter(PropertyInfo property)
        {
            _property = property;
            Name = CommandHelper.GetCommandParameterName(property);
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.Property(x => x.Name.FirstLongest().ToString());
            b.Property(x => x.Type.ToPrettyString(false));
            b.Property(x => x.Position);
        });

        public SoftKeySet Name { get; }

        public Type Type => _property.PropertyType;

        [CanBeNull]
        public int? Position => _property.GetCustomAttribute<PositionAttribute>()?.Value;

        [CanBeNull]
        public object DefaultValue => _property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        
        public bool IsRequired => _property.IsDefined(typeof(RequiredAttribute)) || Position > CommandLine.CommandIndex;

        [NotNull]
        public static CommandParameter Create(PropertyInfo property) => new CommandParameter(property);

        public void SetValue(object obj, object value) => _property.SetValue(obj, value);
    }
}