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
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandArgumentMetadata
    {
        private CommandArgumentMetadata(PropertyInfo member)
        {
            Property = member;
            Position = member.GetCustomAttribute<PositionAttribute>()?.Value;
            Name =
                Position.HasValue
                    ? NameSet.FromPosition(Position.Value)
                    : CommandHelper.GetCommandParameterId(member);
            Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description;
            DefaultValue = member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            Required = member.IsDefined(typeof(RequiredAttribute)) || Name.Default.Option.Contains(Reusable.Commander.Name.Options.Positional);
            ConverterType = member.GetCustomAttribute<Reusable.OneTo1.TypeConverterAttribute>()?.ConverterType;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayScalar(x => x.Name.Default.ToString());
            b.DisplayScalar(x => x.Property.PropertyType.ToPrettyString(false));
            b.DisplayScalar(x => x.Position);
        });

        [NotNull]
        public PropertyInfo Property { get; }

        [NotNull]
        public NameSet Name { get; }

        [CanBeNull]
        public string Description { get; }

        [CanBeNull]
        public int? Position { get; }

        [CanBeNull]
        public object DefaultValue { get; }

        public bool Required { get; }
        
        public Type ConverterType { get; }

        [NotNull]
        public static CommandArgumentMetadata Create([NotNull] PropertyInfo property)
        {
            return new CommandArgumentMetadata(property ?? throw new ArgumentNullException(nameof(property)));
        }

        //public void SetValue(object obj, object value) => Property.SetValue(obj, value);
    }
}