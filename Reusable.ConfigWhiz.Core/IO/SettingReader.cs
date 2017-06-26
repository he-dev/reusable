using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.IO
{
    internal class SettingReader
    {
        public SettingReader(SettingProxy setting, TypeConverter converter, IImmutableList<IDatastore> datastores)
        {
            Setting = setting;
            Converter = converter;
            Datastores = datastores;
        }

        public TypeConverter Converter { get; set; }

        public IImmutableList<IDatastore> Datastores { get; }

        public SettingProxy Setting { get; }

        public IDatastore CurrentStore { get; private set; }

        public void Read()
        {
            var (store, settings) =
                CurrentStore == null
                    ? Resolve()
                    : (CurrentStore, CurrentStore.Read(Setting.Identifier));

            var values = GetValues(settings);
            var value = Convert(values);

            foreach (var validation in Setting.Validations)
            {
                validation.Validate(value, Setting.Identifier.ToString());
            }

            CurrentStore = CurrentStore ?? store;
            Setting.Value = value;
        }

        private (IDatastore store, ICollection<ISetting> settings) Resolve()
        {
            // Try to use a named datastore first.
            if (Setting.DefaultDatastore.IsNotNull())
            {
                return (Setting.DefaultDatastore, Setting.DefaultDatastore?.Read(Setting.Identifier));
            }

            // Probe each datastore for the setting.
            foreach (var store in Datastores)
            {
                var settings = store.Read(Setting.Identifier);
                if (settings.Any()) return (store, settings);
            }

            if (Setting.FallbackDatastore.IsNotNull())
            {
                return (Setting.FallbackDatastore, Setting.FallbackDatastore?.Read(Setting.Identifier));
            }

            throw new DatastoreNotFoundException(Setting.Identifier);
        }

        private object GetValues(ICollection<ISetting> settings)
        {
            if (Setting.IsItemized)
            {
                if (Setting.Type.IsDictionary()) return settings.ToDictionary(x => x.Identifier.Element, x => x.Value);
                if (Setting.Type.IsEnumerable()) return settings.Select(x => x.Value);
                throw new UnsupportedItemizedTypeException(Setting.Identifier, Setting.Type);
            }

            if (settings.Count > 1)
            {
                throw new MultipleSettingMatchesException(Setting.Identifier, CurrentStore);
            }

            return settings.SingleOrDefault()?.Value;
        }

        private object Convert(object value)
        {
            return value == null ? null : Converter.Convert(value, Setting.Type, Setting.Format?.FormatString, Setting.Format?.FormatProvider ?? CultureInfo.InvariantCulture);
        }
    }

    public class UnsupportedItemizedTypeException : Exception
    {
        public UnsupportedItemizedTypeException(Identifier identifier, Type settingType)
            : base($"'{settingType}' type used by '{identifier}' setting is not supported for itemized settings. You can use either {nameof(IDictionary)} or {nameof(IEnumerable)}.")
        { }
    }

    public class MultipleSettingMatchesException : Exception
    {
        public MultipleSettingMatchesException(Identifier identifier, IDatastore datastore)
            : base($"Found multiple matches of '{identifier}' in '{datastore.Name}'  but expected one.")
        { }
    }

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(Identifier identifier)
            : base($"Could not find datastore for '{identifier}'")
        { }
    }
}