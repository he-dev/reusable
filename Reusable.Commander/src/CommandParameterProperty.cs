using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Commander
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandParameterProperty
    {
        private CommandParameterProperty(PropertyInfo member)
        {
            Info = member;

            //Names = CommandHelper.GetParameterNames(member).ToList();
            Type = member.PropertyType;
            IsCollection = typeof(IList<string>).IsAssignableFrom(member.PropertyType);
            Id = CommandHelper.GetCommandParameterId(member);
            Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description;
            Position = member.GetCustomAttribute<PositionAttribute>()?.Value;
            DefaultValue = Reflection.Utilities.FindCustomAttributes<DefaultValueAttribute>(member)?.SingleOrDefault()?.Value;
            Required = member.IsDefined(typeof(RequiredAttribute)) || Position.HasValue;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayMember(x => x.Id.Default.ToString());
            b.DisplayMember(x => x.Type.ToPrettyString(false));
            b.DisplayMember(x => x.Position);
        });

        [NotNull]
        private PropertyInfo Info { get; }
        
        [NotNull]
        public Type Type { get; }
        
        public bool IsCollection { get; }

        [NotNull]
        public Identifier Id { get; }


        [CanBeNull]
        public string Description { get; }

        [CanBeNull]
        public int? Position { get; }

        [CanBeNull]
        public object DefaultValue { get; }

        public bool Required { get; }

        [NotNull]
        public static CommandParameterProperty Create([NotNull] PropertyInfo property) => new CommandParameterProperty(property ?? throw new ArgumentNullException(nameof(property)));

        public void SetValue(object obj, object value) => Info.SetValue(obj, value);
    }
}