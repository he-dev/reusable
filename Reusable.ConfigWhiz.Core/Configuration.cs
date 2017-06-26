using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public interface IConfiguration
    {
        [CanBeNull]
        TContainer Resolve<TContainer>(Identifier identifier, DataOrigin dataOrigin = DataOrigin.Cache) where TContainer : class, new();
    }

    public class Configuration : IConfiguration
    {
        private readonly IImmutableList<IDatastore> _datastores;

        private readonly AutoKeyDictionary<Identifier, SettingContainer> _containers = new AutoKeyDictionary<Identifier, SettingContainer>(x => x.Identifier);

        public Configuration(params IDatastore[] datastores)
            : this((IEnumerable<IDatastore>)datastores)
        { }

        public Configuration(IEnumerable<IDatastore> datastores)
        {
            var builder = ImmutableList.CreateBuilder<IDatastore>();
            foreach (var store in datastores)
            {
                if (builder.Contains(store))
                {
                    throw new DuplicateDatatastoreException(store);
                }
                builder.Add(store);
            }
            _datastores = builder.ToImmutable();

            if (!_datastores.Any())
            {
                throw new ArgumentException("You need to specify at least one datastore.");
            }
        }

        public static readonly TypeConverter DefaultConverter = TypeConverterFactory.CreateDefaultConverter();

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public TContainer Resolve<TContainer>(Identifier identifier, DataOrigin dataOrigin) where TContainer : class, new()
        {
            if (_containers.TryGetValue(identifier, out SettingContainer container))
            {
                if (dataOrigin == DataOrigin.Provider)
                {
                    container.Load();
                }                
            }
            else
            {
                container = SettingContainer<TContainer>.Create(identifier, _datastores);
                container.Load();
                _containers.Add(container);
            }
            return container.Value as TContainer;
        }

        public void Save()
        {
            foreach (var container in _containers)
            {
                container.Value.Save();
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

    //public class ConsumerName
    //{
    //    private readonly string _name;
    //    private ConsumerName(string name) { _name = name; }
    //    public static readonly ConsumerName Any = new ConsumerName(string.Empty);
    //    public override string ToString() => _name;
    //    public static implicit operator string(ConsumerName consumerName) => consumerName._name;
    //}

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
