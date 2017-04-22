using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Converters;
using Reusable.ConfigWhiz.Data;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
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

        public int Write(IDatastore datastore)
        {
            var data = Serialize(datastore);
            var group = new SettingGroup(Setting.Path, data);
            return datastore.Write(group);            
        }

        private ICollection<ISetting> Serialize(IDatastore datastore)
        {
            if (Setting.IsItemized)
            {
                var settingType = 
                    Setting.Type.IsDictionary() 
                        ? Setting.Type.GetGenericArguments()[1] // a dictionary's value type
                        : Setting.Type.GetElementType();

                var storeType = GetDataType(settingType);
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
}