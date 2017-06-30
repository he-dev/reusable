using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Converters;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.IO;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class SettingConverter
    {
        private readonly TypeConverter _converter;

        public SettingConverter(TypeConverter converter)
        {
            _converter = converter;
        }

        private static TypeConverter Itemizer { get; } = TypeConverter.Empty.Add<EnumerableToDictionaryConverter>();

        public object Deserialize(Setting setting, ICollection<IEntity> entities)
        {
            var values = GetValues(entities, setting.Itemized, setting.Type);
            var value = values == null ? null : _converter.Convert(values, setting.Type, setting.Format?.FormatString, setting.Format?.FormatProvider ?? CultureInfo.InvariantCulture);

            foreach (var validation in setting.Validations)
            {
                validation.Validate(value, setting.Id.ToString());
            }

            return value;
        }

        private static object GetValues(ICollection<IEntity> settings, bool itemized, Type type)
        {
            if (itemized)
            {
                if (type.IsDictionary()) return settings.ToDictionary(x => x.Id.Last().Value, x => x.Value);
                if (type.IsEnumerable()) return settings.Select(x => x.Value);
                throw new UnsupportedItemizedTypeException(null, type);
            }

            if (settings.Count > 1)
            {
                throw new MultipleSettingMatchesException(null, null);
            }

            return settings.SingleOrDefault()?.Value;
        }

        public IGrouping<IIdentifier, IEntity> Serialize(Setting setting, IImmutableSet<Type> supportedTypes)
        {
            if (setting.Itemized)
            {
                var elementType =
                    setting.Type.IsDictionary()
                        ? setting.Type.GetGenericArguments()[1] // a dictionary's value type
                        : setting.Type.GetElementType();

                var targetType = ResolveDataType(elementType, supportedTypes);
                var items = (IDictionary)Convert(setting, Itemizer, typeof(Dictionary<object, object>));
                var entities =
                    from key in items.Keys.Cast<object>()
                    let element = (string) _converter.Convert(key, typeof(string))
                    select new Entity
                    {
                        Id = new Identifier(setting.Id.Concat(new IToken[] {new Token(element, TokenType.Element)})),
                        Value = _converter.Convert(items[key], targetType)
                    };

                return new SettingGroup(setting.Id, entities.ToList());
            }
            else
            {
                var settingType = ResolveDataType(setting.Type, supportedTypes);
                var value = Convert(setting, _converter, settingType);
                var entities = new[]
                {
                    new Entity
                    {
                        Id = setting.Id,
                        Value = value
                    }
                };
                return new SettingGroup(setting.Id, entities);
            }


        }

        private static object Convert(Setting setting, TypeConverter converter, Type targetType)
        {
            return converter.Convert(
                setting.Value,
                targetType,
                setting.Format?.FormatString,
                setting.Format?.FormatProvider ?? CultureInfo.InvariantCulture);
        }

        private static Type ResolveDataType(Type settingType, IImmutableSet<Type> supportedTypes)
        {
            var fallbackType = typeof(string);

            return
                new[] { settingType, fallbackType }.FirstOrDefault(supportedTypes.Contains)
                ?? throw new NotSupportedException($"'{settingType}' is not a supported data type."); ;
        }
    }
}