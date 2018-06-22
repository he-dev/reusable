using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public abstract partial class SettingDataStore : ISettingDataStore
    {
        private static readonly ConcurrentDictionary<SoftString, int> InstanceCounters = new ConcurrentDictionary<SoftString, int>();

        private readonly ISettingConverter _converter;

        private ISettingNameGenerator _settingNameGenerator;

        private SoftString _name;

        private SettingDataStore()
        {
            Name = CreateDefaultName(GetType());
            SettingNameGenerator = new SettingNameByUsageGenerator();
        }

        protected SettingDataStore([NotNull] ISettingConverter converter) : this()
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public ISettingNameGenerator SettingNameGenerator
        {
            get => _settingNameGenerator;
            set => _settingNameGenerator = value ?? throw new ArgumentNullException(nameof(SettingNameGenerator));
        }

        public ISetting Read(SoftString settingName, Type settingType)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            // Materialize the generated names because we'll be using it multiple times.
            var names = 
                SettingNameGenerator
                    .GenerateSettingNames(settingName)
                    .Select(name => (SoftString)(string)name)
                    .ToList();
            try
            {
                var setting = ReadCore(names);

                return setting is null ? null : new Setting(setting.Name)
                {
                    Value = settingType is null ? setting.Value : (setting.Value is null ? null : _converter.Deserialize(setting.Value, settingType))
                };
            }
            catch (Exception innerException)
            {
                throw ($"ReadSetting{nameof(Exception)}", $"Cannot read {settingName.ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        [CanBeNull]
        protected abstract ISetting ReadCore(IReadOnlyCollection<SoftString> names);

        public void Write(ISetting setting)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));

            try
            {
                WriteCore(new Setting(setting.Name)
                {
                    Value = setting.Value is null ? null : _converter.Serialize(setting.Value)
                });
            }
            catch (Exception innerException)
            {
                throw ($"WriteSetting{nameof(Exception)}", $"Cannot write {setting.Name.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void WriteCore(ISetting setting);

        private static string CreateDefaultName(Type datastoreType)
        {
            return datastoreType.ToPrettyString() +  InstanceCounters.AddOrUpdate(datastoreType.ToPrettyString(), name => 1, (name, counter) => counter + 1);
        }

        protected Exception CreateAmbiguousSettingException(IEnumerable<SoftString> names)
        {
            throw DynamicException.Factory.CreateDynamicException(
                $"AmbiguousSetting{nameof(Exception)}",
                $"Mutliple settings found: {names.Select(name => name.ToString()).Join(", ").EncloseWith("[]")}",
                null
            );
        }
    }

    public abstract partial class SettingDataStore
    {
        public override int GetHashCode()
        {
            return AutoEquality<ISettingDataStore>.Comparer.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            return obj is ISettingDataStore other && Equals(other);
        }

        public bool Equals(ISettingDataStore other)
        {
            return AutoEquality<ISettingDataStore>.Comparer.Equals(this, other);
        }
    }
}