using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.ConfigWhiz.Extensions;
using Reusable.ConfigWhiz.Paths;
using Reusable.Data.Annotations;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Data
{
    public class Setting
    {
        private readonly Identifier _identifier;
        private readonly object _container;
        private readonly PropertyInfo _property;

        public Setting(Identifier identifier, object container, PropertyInfo property)
        {
            _identifier = identifier;
            _container = container;
            _property = property;
        }

        public Identifier Identifier => Identifier.From(_identifier, _property.GetCustomNameOrDefault());

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

    internal class SettingGroup : List<IEntity>, IGrouping<Identifier, IEntity>
    {
        public SettingGroup(Identifier key, IEnumerable<IEntity> settings) : base(settings) => Key = key;
        public Identifier Key { get; }
    }
}