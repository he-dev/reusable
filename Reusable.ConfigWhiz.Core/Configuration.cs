using System;
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

        public Configuration(IImmutableList<IDatastore> settingStores)
        {
            _settingStores = settingStores;
        }

        public static readonly TypeConverter DefaultConverter = TypeConverterFactory.CreateDefaultConverter();

        #region Load overloads

        public Result<TContainer> Load<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName, LoadOption loadOption) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(selectConsumerName(consumer), loadOption);
        }

        public Result<TContainer> Load<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> selectConsumerName) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(selectConsumerName(consumer), LoadOption.Cached);
        }

        public Result<TContainer> Load<TConsumer, TContainer>(LoadOption loadOption) where TContainer : new()
        {
            return Load<TConsumer, TContainer>(ConsumerName.Any, loadOption);
        }

        public Result<TContainer> Load<TConsumer, TContainer>() where TContainer : new()
        {
            return Load<TConsumer, TContainer>(ConsumerName.Any, LoadOption.Cached);
        }

        #endregion

        private Result<TContainer> Load<TConsumer, TContainer>(object consumerName, LoadOption loadOption) where TContainer : new()
        {
            if (consumerName is string s && s.IsNullOrEmpty()) throw new ArgumentNullException(nameof(consumerName));

            var key = ContainerPath.Create<TConsumer, TContainer>(consumerName.ToString());

            if (_containers.TryGetValue(key, out SettingContainer container) && loadOption != LoadOption.Cached)
            {
                container.Load(loadOption);
            }
            else
            {
                container = SettingContainer.Create<TConsumer, TContainer>(consumerName.ToString(), _settingStores);
                var results = container.Load(LoadOption.Resolve);
                _containers.Add(container);

                if (results.Any(x => x.Failure)) return Result<TContainer>.Fail("Could not load one or more settings.", results.Where(x => x.Failure));
            }
            return Result<TContainer>.Ok(container.As<TContainer>());
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

    public class SettingProperty
    {
        private readonly string _name;
        private SettingProperty(string name) { _name = name; }
        public static readonly SettingProperty Name = new SettingProperty(nameof(Name));
        public static readonly SettingProperty Value = new SettingProperty(nameof(Value));
        public override string ToString() => _name;
        public static implicit operator string(SettingProperty settingProperty) => settingProperty._name;
    }
}
