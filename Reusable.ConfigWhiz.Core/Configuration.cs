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
        private readonly IImmutableList<IDatastore> _settingStores;
        private readonly AutoKeyDictionary<ContainerPath, SettingContainer> _containers = new AutoKeyDictionary<ContainerPath, SettingContainer>(x => x.Path);

        public Configuration(IEnumerable<IDatastore> settingStores)
        {
            var builder = ImmutableList.CreateBuilder<IDatastore>();//new DatastoreComparer());
            foreach (var store in settingStores)
            {
                if (builder.Exists(x => x.Name.Equals(store.Name, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException($"Datastore '{store.Name}' already exists.");
                builder.Add(store);
            }
            _settingStores = builder.ToImmutable();
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
                container = SettingContainer.Create<TConsumer, TContainer>(consumerName.ToString(), _settingStores);
                container.Load(LoadOption.Resolve);
                _containers.Add(container);
            }
            return container.As<TContainer>();
        }
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
