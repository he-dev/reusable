using System;
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
                    : ResolveDatastore();

            var data = GetValues(settings);
            var value = Convert(data);

            foreach (var validation in Setting.Validations)
            {
                validation.Validate(value, Setting.Path.ToFullWeakString());
            }

            CurrentStore = CurrentStore ?? store;
            Setting.Value = value;
        }        

        private (IDatastore store, ICollection<ISetting> settings) ResolveDatastore()
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

        private object GetValues(IEnumerable<ISetting> settings)
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

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(SettingPath settingPath)
            : base($"Could not find datastore for '{settingPath.ToFullWeakString()}'")
        { }
    }
}