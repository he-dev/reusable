using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Extensions;

namespace Reusable.SmartConfig.Data
{
    public class Setting
    {
        [NotNull]
        private readonly object _container;

        [NotNull]
        private readonly PropertyInfo _property;

        public Setting(IIdentifier id, object container, PropertyInfo property)
        {
            _container = container;
            _property = property;

            var name = property.GetCustomNameOrDefault();
            Id = SimpleId ? Identifier.Create(Token.Literal(name)) : Identifier.From(id, property.GetCustomNameOrDefault());
        }

        public IIdentifier Id { get; }

        public bool SimpleId => _property.GetCustomAttribute<SimpleSettingAttribute>().IsNotNull();

        public Type Type => _property.PropertyType;

        public bool Itemized => _property.GetCustomAttribute<ItemizedAttribute>().IsNotNull();

        public FormatAttribute Format => _property.GetCustomAttribute<FormatAttribute>();

        public IEnumerable<ValidationAttribute> Validations => _property.GetCustomAttributes<ValidationAttribute>();

        public bool ReadOnly => _property.GetCustomAttribute<ReadOnlyAttribute>().IsNotNull();

        public object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }
    }

    internal class SettingGroup : List<IEntity>, IGrouping<IIdentifier, IEntity>
    {
        public SettingGroup(IIdentifier key, IEnumerable<IEntity> settings) : base(settings) => Key = key;
        public IIdentifier Key { get; }
    }
}