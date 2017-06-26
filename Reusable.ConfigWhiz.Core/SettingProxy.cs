using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.ConfigWhiz.Extensions;
using Reusable.ConfigWhiz.IO;
using Reusable.ConfigWhiz.Paths;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using TypeConverter = Reusable.TypeConversion.TypeConverter;

namespace Reusable.ConfigWhiz
{
    public class SettingProxy
    {
        private readonly Identifier _containerIdentifier;
        private readonly object _container;
        private readonly PropertyInfo _settingProperty;

        private readonly SettingReader _reader;
        private readonly SettingWriter _writer;

        public SettingProxy(Identifier containerIdentifier, object container, PropertyInfo settingProperty, IImmutableList<IDatastore> stores, TypeConverter converter)
        {
            _container = container;
            _containerIdentifier = containerIdentifier;
            _settingProperty = settingProperty;

            _reader = new SettingReader(this, converter, stores);
            _writer = new SettingWriter(this, converter);

            DefaultDatastore = stores.SingleOrDefault(s => s.Equals(_settingProperty.GetCustomAttribute<DefaultDatastoreAttribute>()?.ToString()));
            FallbackDatastore = stores.SingleOrDefault(s => s.Equals(
                _settingProperty.GetCustomAttribute<FallbackDatastoreAttribute>()?.ToString() ??
                _settingProperty.DeclaringType.GetCustomAttribute<FallbackDatastoreAttribute>()?.ToString()));
        }

        public IEnumerable<ValidationAttribute> Validations => _settingProperty.GetCustomAttributes<ValidationAttribute>();

        public bool IsItemized => _settingProperty.GetCustomAttribute<ItemizedAttribute>().IsNotNull();

        public FormatAttribute Format => _settingProperty.GetCustomAttribute<FormatAttribute>();

        public Type Type => _settingProperty.PropertyType;

        public Identifier Identifier => Identifier.From(_containerIdentifier, _settingProperty.GetCustomNameOrDefault());

        public bool ReadOnly => _settingProperty.GetCustomAttribute<ReadOnlyAttribute>().IsNotNull();

        public IDatastore DefaultDatastore { get; }

        public IDatastore FallbackDatastore { get; }

        internal object Value
        {
            get => _settingProperty.GetValue(_container);
            set => _settingProperty.SetValue(_container, value);
        }

        public void Load()
        {
            _reader.Read();
        }

        public int Save()
        {
            return _writer.Write(_reader.CurrentStore);
        }
    }

    internal class SettingGroup : List<ISetting>, IGrouping<Identifier, ISetting>
    {
        public SettingGroup(Identifier key, IEnumerable<ISetting> settings) : base(settings) => Key = key;
        public Identifier Key { get; }
    }
}