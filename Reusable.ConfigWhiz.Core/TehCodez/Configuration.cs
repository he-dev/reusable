using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.SmartConfig.Collections;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Services;
using Reusable.TypeConversion;

namespace Reusable.SmartConfig
{
    public interface IConfiguration
    {
        [CanBeNull]
        TContainer Get<TContainer>(IIdentifier id, bool cached = true) where TContainer : class, new();

        void Save();
    }

    public class Configuration : IConfiguration
    {
        private readonly SettingReader _reader;
        private readonly SettingWriter _writer;

        private readonly IDictionary<IEquatable<IIdentifier>, SettingContainer> _containers = new Dictionary<IEquatable<IIdentifier>, SettingContainer>();

        public Configuration(IEnumerable<IDatastore> datastores) : this(datastores, DefaultConverter) { }

        public Configuration(IEnumerable<IDatastore> datastores, TypeConverter converter)
        {
            datastores = datastores.ToList();
            var duplicateDatastores = datastores.GroupBy(x => x, new DatastoreComparer()).Where(g => g.Count() > 1).Select(g => g.Key.Name).ToList();
            if (duplicateDatastores.Any()) { throw new DuplicateDatatastoreException(duplicateDatastores); }

            var datastoreCache = new DatastoreCache();
            var settingConverter = new SettingConverter(converter);
            _reader = new SettingReader(datastoreCache, settingConverter, datastores);
            _writer = new SettingWriter(datastoreCache, settingConverter);
        }

        public static ConfigurationBuilder Builder => new ConfigurationBuilder();

        public static readonly TypeConverter DefaultConverter = TypeConverterFactory.CreateDefaultConverter();

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public TContainer Get<TContainer>(IIdentifier id, bool cached) where TContainer : class, new()
        {
            var container = GetContainer<TContainer>(id);
            return _reader.Read(container, cached).As<TContainer>();
        }

        private SettingContainer GetContainer<TContainer>(IIdentifier identifier) where TContainer : class, new()
        {
            return _containers.TryGetValue(identifier, out var container) ? container : Cache(SettingContainer.Create<TContainer>(identifier));
        }

        private SettingContainer Cache(SettingContainer settingContainer)
        {
            _containers.Add(settingContainer, settingContainer);
            return settingContainer;
        }

        public void Save()
        {
            foreach (var container in _containers.Values)
            {
                _writer.Write(container);
            }
        }
    }

    public class DuplicateDatatastoreException : Exception
    {
        public DuplicateDatatastoreException(IEnumerable<string> datastoreNames)
            : base($"Duplicate datastore names found: [{string.Join(", ", datastoreNames)}].")
        { }
    }
}
