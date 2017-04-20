using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Converters;
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

        public SettingProxy(object container, ContainerPath containerPath, PropertyInfo property, IImmutableSet<IDatastore> stores, TypeConverter converter)
        {
            _container = container;
            _containerPath = containerPath;
            _property = property;

            _reader = new SettingReader(this, converter, stores);
            _writer = new SettingWriter(this, converter);
        }

        public IEnumerable<ValidationAttribute> Validations => _property.GetCustomAttributes<ValidationAttribute>();

        public bool IsItemized => _property.GetCustomAttribute<ItemizedAttribute>().IsNotNull();

        public FormatAttribute Format => _property.GetCustomAttribute<FormatAttribute>();

        public Type Type => _property.PropertyType;

        public SettingPath Path => SettingPath.Create(_containerPath, _property, string.Empty);

        internal object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        public Result Load(LoadOption loadOption)
        {
            return _reader.Read(loadOption);
        }

        public Result Save()
        {
            return _writer.Write(_reader.CurrentStore);
        }
    }

    internal class SettingReader
    {
        public SettingReader(SettingProxy setting, TypeConverter converter, IImmutableSet<IDatastore> datastores)
        {
            Setting = setting;
            Converter = converter;
            Datastores = datastores;
        }

        public TypeConverter Converter { get; set; }

        public IImmutableSet<IDatastore> Datastores { get; }

        public SettingProxy Setting { get; }

        public IDatastore CurrentStore { get; private set; }

        public Result Read(LoadOption loadOption)
        {
            if (loadOption == LoadOption.Update && CurrentStore.IsNotNull())
            {
                return Try.Execute(Update);
            }
            else
            {
                return Try.Execute(Resolve);
            }
        }

        private bool Update()
        {
            var value = Read(CurrentStore);
            Setting.Value = value;
            return Result.Ok();
        }

        private bool Resolve()
        {
            // Try to load the setting with each datastore and pick the first one that succeeded.
            var result =
                (from store in Datastores
                 let value = Read(store)
                 where value != null
                 select new { value, store }).FirstOrDefault();

            foreach (var validation in Setting.Validations)
            {
                validation.Validate(result?.value, Setting.Path.ToFullWeakString());
            }

            if (result == null) { return false; }

            CurrentStore = result.store;
            Setting.Value = result.value;
            return true;
        }

        private object Read(IDatastore store)
        {
            var settings = store.Read(Setting.Path);
            if (!settings.Any())
            {
                return null;
            }
            var data = GetData(settings);
            var value = Convert(data);
            return value;
        }

        private object GetData(ICollection<ISetting> settings)
        {
            if (Setting.IsItemized)
            {
                if (Setting.Type.IsDictionary()) return settings.ToDictionary(x => x.Path.ElementName, x => x.Value);
                if (Setting.Type.IsEnumerable()) return settings.Select(x => x.Value);
                throw new ArgumentException($"'{Setting.Path.ToFullWeakString()}' uses a type '{Setting.Type}' that is not supported for itemized settings.");
            }
            else
            {
                return settings.SingleOrDefault()?.Value;
            }
        }

        private object Convert(object value)
        {
            return value == null ? null : Converter.Convert(value, Setting.Type, Setting.Format?.FormatString, Setting.Format?.FormatProvider ?? CultureInfo.InvariantCulture);
        }
    }

    internal class SettingWriter
    {
        public SettingWriter(SettingProxy setting, TypeConverter converter)
        {
            Setting = setting;
            Converter = converter;
            Itemizer = TypeConverter.Empty.Add<EnumerableToDictionaryConverter>(); ;
        }

        private TypeConverter Itemizer { get; }

        private TypeConverter Converter { get; }

        private SettingProxy Setting { get; }

        public Result Write(IDatastore datastore)
        {
            var data = Serialize(datastore);
            var group = new SettingGroup(Setting.Path, data);
            datastore.Write(group);
            return Result.Ok();
        }

        private ICollection<ISetting> Serialize(IDatastore datastore)
        {
            if (Setting.IsItemized)
            {
                var storeType = GetDataType(Setting.Type.GetElementType());
                var items = (IDictionary)Convert(Itemizer, typeof(Dictionary<object, object>));
                var settings = items.Keys.Cast<object>().Select(key => new Setting
                {
                    Path = new SettingPath(
                        Setting.Path.ConsumerNamespace,
                        Setting.Path.ConsumerName,
                        Setting.Path.InstanceName,
                        Setting.Path.ContainerName,
                        Setting.Path.SettingName,
                        elementName: (string)Converter.Convert(key, typeof(string))),
                    Value = Converter.Convert(items[key], storeType)
                })
                .Cast<ISetting>().ToList();

                return settings;
            }
            else
            {
                var settingType = GetDataType(Setting.Type);
                var value = Convert(Converter, settingType);
                var settings = new[]
                {
                    new Setting
                    {
                        Path = Setting.Path,
                        Value = value
                    }
                };
                return settings;
            }

            Type GetDataType(Type settingType)
            {
                return
                    new[] { settingType, typeof(string) }.FirstOrDefault(datastore.SupportedTypes.Contains)
                    ?? throw new NotSupportedException($"'{Setting.Type.GetElementType()}' is not a supported data type."); ;
            }

            object Convert(TypeConverter converter, Type targetType)
            {
                return converter.Convert(
                    Setting.Value,
                    targetType,
                    Setting.Format?.FormatString,
                    Setting.Format?.FormatProvider ?? CultureInfo.InvariantCulture);
            }
        }
    }

    internal class SettingGroup : List<ISetting>, IGrouping<SettingPath, ISetting>
    {
        public SettingGroup(SettingPath key, IEnumerable<ISetting> settings) : base(settings) => Key = key;
        public SettingPath Key { get; }
    }
}