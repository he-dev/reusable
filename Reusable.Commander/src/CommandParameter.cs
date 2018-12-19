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
        private CommandParameter(PropertyInfo property)
        {
            Property = property;
            Id = CommandHelper.GetCommandParameterId(property);
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayMember(x => x.Id.Default.ToString());
            b.DisplayMember(x => x.Type.ToPrettyString(false));
            b.DisplayMember(x => x.Position);
        });

        [NotNull]
        private PropertyInfo Property { get; }

        [NotNull]
        public Identifier Id { get; }
        
        [NotNull]
        public Type Type => Property.PropertyType;
        
        [CanBeNull]
        public string Description => Property.GetCustomAttribute<DescriptionAttribute>()?.Description;

        [CanBeNull]
        public int? Position => Property.GetCustomAttribute<PositionAttribute>()?.Value;

        [CanBeNull]
        public object DefaultValue => Property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        
        public bool IsRequired => Property.IsDefined(typeof(RequiredAttribute)) || Position.HasValue;

        [NotNull]
        public static CommandParameter Create([NotNull] PropertyInfo property) => new CommandParameter(property ?? throw new ArgumentNullException(nameof(property)));

        public void SetValue(object obj, object value) => Property.SetValue(obj, value);
    }
}