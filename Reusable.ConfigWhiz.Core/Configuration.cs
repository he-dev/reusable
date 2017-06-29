using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.IO;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class DatastoreCache
    {
        private readonly IDictionary<Identifier, IDatastore> _datastores = new Dictionary<Identifier, IDatastore>();

        public bool TryGetDatastore(Identifier identifier, out IDatastore datastore)
        {
            return _datastores.TryGetValue(identifier, out datastore);
        }

        public void Add(Identifier identifier, IDatastore datastore)
        {
            _datastores.Add(identifier, datastore);
        }

        public void Remove(Identifier identifier)
        {
            _datastores.Remove(identifier);
        }

        public bool Contains(Identifier identifier) => _datastores.ContainsKey(identifier);
    }

    public interface IConfiguration
    {
        [CanBeNull]
        TContainer Get<TContainer>(Identifier identifier, bool cached = true) where TContainer : class, new();
    }
        
    public class Configuration : IConfiguration
    {
        private readonly SettingReader _reader;
        private readonly SettingWriter _writer;

        private readonly IDictionary<IEquatable<Identifier>, SettingContainer> _containers = new Dictionary<IEquatable<Identifier>, SettingContainer>();
        
        public Configuration(IEnumerable<IDatastore> datastores) : this(datastores, DefaultConverter)
        { }
        
            public Configuration(IEnumerable<IDatastore> datastores, TypeConverter converter)
        {
            datastores = datastores.ToList();
            var datastoreCache = new DatastoreCache();
            var settingConverter = new SettingConverter(converter);
            _reader = new SettingReader(datastoreCache, settingConverter, datastores);
            _writer = new SettingWriter(datastoreCache, settingConverter);
        }

        //public Configuration(IEnumerable<IDatastore> datastores)
        //{
        //    var builder = ImmutableList.CreateBuilder<IDatastore>();
        //    foreach (var store in datastores)
        //    {
        //        if (builder.Contains(store))
        //        {
        //            throw new DuplicateDatatastoreException(store);
        //        }
        //        builder.Add(store);
        //    }
        //    _datastores = builder.ToImmutable();

        //    if (!_datastores.Any())
        //    {
        //        throw new ArgumentException("You need to specify at least one datastore.");
        //    }
        //}

        public static readonly TypeConverter DefaultConverter = TypeConverterFactory.CreateDefaultConverter();

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public TContainer Get<TContainer>(Identifier identifier, bool cached) where TContainer : class, new()
        {
            var container = GetContainer<TContainer>(identifier);
            return _reader.Read(container, cached).As<TContainer>();            
        }

        private SettingContainer GetContainer<TContainer>(Identifier identifier) where TContainer : class, new()
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
        public DuplicateDatatastoreException(IDatastore datastore)
            : base($"Another datastore with the name '{datastore.Name}' already exists.")
        { }
    }

    public enum DataOrigin
    {
        Cache,
        Provider
    }

    public class SettingProperty
    {
        private readonly string _name;
        private SettingProperty(string name) { _name = name; }
        public static readonly SettingProperty Name = new SettingProperty(nameof(Name));
        public static readonly SettingProperty Value = new SettingProperty(nameof(Value));
        //public static readonly IEnumerable<SettingProperty> Default = new[] { Name, Value };
        public static bool Exists(string name) =>
            name.Equals(nameof(Name), StringComparison.OrdinalIgnoreCase) ||
            name.Equals(nameof(Value), StringComparison.OrdinalIgnoreCase);
        public override string ToString() => _name;
        public static implicit operator string(SettingProperty settingProperty) => settingProperty._name;
    }
}
