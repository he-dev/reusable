using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
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

        public void Read(LoadOption loadOption)
        {
            var (store, settings) =
                loadOption == LoadOption.Update && CurrentStore.IsNotNull()
                    ? (CurrentStore, CurrentStore.Read(Setting.Path))
                    : Resolve();

            var values = GetValues(settings);
            var value = Convert(values);

            foreach (var validation in Setting.Validations)
            {
                validation.Validate(value, Setting.Path.ToFullWeakString());
            }

            CurrentStore = CurrentStore ?? store;
            Setting.Value = value;
        }

        private (IDatastore store, ICollection<ISetting> settings) Resolve()
        {
            // Try to use a named datastore first.
            if (Setting.DefaultDatastore.IsNotNull())
            {
                return (Setting.DefaultDatastore, Setting.DefaultDatastore?.Read(Setting.Path));
            }

            // Probe each datastore for the setting.
            foreach (var store in Datastores)
            {
                var settings = store.Read(Setting.Path);
                if (settings.Any()) return (store, settings);
            }

            if (Setting.FallbackDatastore.IsNotNull())
            {
                return (Setting.FallbackDatastore, Setting.FallbackDatastore?.Read(Setting.Path));
            }

            throw new DatastoreNotFoundException(Setting.Path);
        }

        private object GetValues(ICollection<ISetting> settings)
        {
            if (Setting.IsItemized)
            {
                if (Setting.Type.IsDictionary()) return settings.ToDictionary(x => x.Path.ElementName, x => x.Value);
                if (Setting.Type.IsEnumerable()) return settings.Select(x => x.Value);
                throw new UnsupportedItemizedTypeException(Setting.Path, Setting.Type);
            }

            if (settings.Count > 1)
            {
                throw new MultipleSettingMatchesException(Setting.Path, CurrentStore);
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
        public UnsupportedItemizedTypeException(SettingPath settingPath, Type settingType)
            : base($"'{settingType}' type used by '{settingPath.ToFullWeakString()}' setting is not supported for itemized settings. You can use either {nameof(IDictionary)} or {nameof(IEnumerable)}.")
        { }
    }

    public class MultipleSettingMatchesException : Exception
    {
        public MultipleSettingMatchesException(SettingPath settingPath, IDatastore datastore)
            : base($"Found multiple matches of '{settingPath.ToFullWeakString()}' in '{datastore.Name}'  but expected one.")
        { }
    }

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(SettingPath settingPath)
            : base($"Could not find datastore for '{settingPath.ToFullWeakString()}'")
        { }
    }
}