using System;
using System.Collections.Immutable;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class Configuration
    {
        private readonly IImmutableList<IDatastore> _settingStores;
        private readonly AutoKeyDictionary<ContainerPath, SettingContainer> _containers = new AutoKeyDictionary<ContainerPath, SettingContainer>(x => x.Path);

        public Configuration(IImmutableList<IDatastore> settingStores)
        {
            _settingStores = settingStores;
        }

        public static readonly TypeConverter DefaultConverter = TypeConverterFactory.CreateDefaultConverter();

        #region Get overloads

        public TContainer Get<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName, LoadOption loadOption) where TContainer : new()
        {
            return Get<TConsumer, TContainer>(selectConsumerName(consumer), loadOption);
        }

        public TContainer Get<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName) where TContainer : new()
        {
            return Get<TConsumer, TContainer>(selectConsumerName(consumer), LoadOption.Update);
        }

        public TContainer Get<TConsumer, TContainer>(LoadOption loadOption) where TContainer : new()
        {
            return Get<TConsumer, TContainer>(ConsumerName.Any, loadOption);
        }

        public TContainer Get<TConsumer, TContainer>() where TContainer : new()
        {
            return Get<TConsumer, TContainer>(ConsumerName.Any, LoadOption.Cached);
        }

        #endregion

        private TContainer Get<TConsumer, TContainer>(object consumerName, LoadOption loadOption) where TContainer : new()
        {
            if (consumerName is string s && s.IsNotNullOrEmpty()) throw new ArgumentNullException(nameof(consumerName));

            var key = ContainerPath.Create<TConsumer, TContainer>(consumerName.ToString());

            if (_containers.TryGetValue(key, out SettingContainer container) && loadOption != LoadOption.Cached)
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

    public class LoadOption
    {
        private LoadOption() { }
        public static readonly LoadOption Resolve = new LoadOption();
        public static readonly LoadOption Update = new LoadOption();
        public static readonly LoadOption Cached = new LoadOption();
    }

    public class ConsumerName
    {
        private readonly string _name;
        private ConsumerName(string name) { _name = name; }
        public static readonly ConsumerName Any = new ConsumerName(string.Empty);
        public override string ToString() => _name;
        public static implicit operator string(ConsumerName consumerName) => consumerName._name;
    }
}
