using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class Configuration
    {
        private readonly IImmutableList<IDatastore> _datastores;
        private readonly AutoKeyDictionary<ContainerPath, SettingContainer> _containers = new AutoKeyDictionary<ContainerPath, SettingContainer>(x => x.Path);

        public Configuration(params IDatastore[] datastores) : this((IEnumerable<IDatastore>)datastores) { }

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

        #region Load overloads

        public TContainer Load<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName, LoadOption loadOption) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(selectConsumerName(consumer), loadOption);
        }

        public TContainer Load<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(selectConsumerName(consumer), LoadOption.Retrieve);
        }

        public TContainer Load<TConsumer, TContainer>(LoadOption loadOption) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(ConsumerName.Any, loadOption);
        }

        public TContainer Load<TConsumer, TContainer>() where TContainer : new()
        {
            return Load<TConsumer, TContainer>(ConsumerName.Any, LoadOption.Retrieve);
        }

        #endregion

        private TContainer Load<TConsumer, TContainer>(object consumerName, LoadOption loadOption) where TContainer : new()
        {
            if (consumerName is string s && s.IsNullOrEmpty()) throw new ArgumentNullException(nameof(consumerName));

            var key = ContainerPath.Create<TConsumer, TContainer>(consumerName.ToString());

            if (_containers.TryGetValue(key, out SettingContainer container))
            {
                container.Load(loadOption);
            }
            else
            {
                container = SettingContainer<TContainer>.Create<TConsumer>(consumerName.ToString(), _datastores);
                container.Load(LoadOption.Resolve);
                _containers.Add(container);
            }
            return (SettingContainer<TContainer>)container;
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

    public abstract class LoadOption
    {
        private LoadOption() { }
        public static readonly LoadOption Resolve = new ResolveOption();
        public static readonly LoadOption Update = new UpdateOption();
        public static readonly LoadOption Retrieve = new RetrieveOption();
        private class ResolveOption : LoadOption { }
        private class UpdateOption : LoadOption { }
        private class RetrieveOption : LoadOption { }
    }

    public class ConsumerName
    {
        private readonly string _name;
        private ConsumerName(string name) { _name = name; }
        public static readonly ConsumerName Any = new ConsumerName(string.Empty);
        public override string ToString() => _name;
        public static implicit operator string(ConsumerName consumerName) => consumerName._name;
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
