using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class SettingProxy
    {
        private readonly ContainerPath _containerPath;
        private readonly object _container;
        private readonly PropertyInfo _property;

        private readonly SettingReader _reader;
        private readonly SettingWriter _writer;

        public SettingProxy(object container, ContainerPath containerPath, PropertyInfo property, IImmutableList<IDatastore> stores, TypeConverter converter)
        {
            _container = container;
            _containerPath = containerPath;
            _property = property;

            _reader = new SettingReader(this, converter, stores);
            _writer = new SettingWriter(this, converter);

            DefaultDatastore = stores.SingleOrDefault(s => s.Equals(_property.GetCustomAttribute<DefaultDatastoreAttribute>()?.ToString()));
            FallbackDatastore = stores.SingleOrDefault(s => s.Equals(
                _property.GetCustomAttribute<FallbackDatastoreAttribute>()?.ToString() ??
                _property.DeclaringType.GetCustomAttribute<FallbackDatastoreAttribute>()?.ToString()));
        }

        public IEnumerable<ValidationAttribute> Validations => _property.GetCustomAttributes<ValidationAttribute>();

        public bool IsItemized => _property.GetCustomAttribute<ItemizedAttribute>().IsNotNull();

        public FormatAttribute Format => _property.GetCustomAttribute<FormatAttribute>();

        public Type Type => _property.PropertyType;

        public SettingPath Path => SettingPath.Create(_containerPath, _property, string.Empty);

        public IDatastore DefaultDatastore { get; }

        public IDatastore FallbackDatastore { get; }

        internal object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        public void Load(LoadOption loadOption)
        {
            _reader.Read(loadOption);
        }

        public Result Save()
        {
            return _writer.Write(_reader.CurrentStore);
        }
    }

    internal class SettingGroup : List<ISetting>, IGrouping<SettingPath, ISetting>
    {
        public SettingGroup(SettingPath key, IEnumerable<ISetting> settings) : base(settings) => Key = key;
        public SettingPath Key { get; }
    }
}