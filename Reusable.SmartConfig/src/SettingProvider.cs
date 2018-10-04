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
    public abstract class SettingProvider : ISettingProvider
    {
        private static readonly ConcurrentDictionary<SoftString, int> InstanceCounters = new ConcurrentDictionary<SoftString, int>();

        private readonly ISettingConverter _converter;

        private ISettingNameFactory _settingNameFactory;

        private SoftString _name;

        private SettingProvider()
        {
            Name = CreateDefaultName(GetType());
        }

        protected SettingProvider([NotNull] ISettingConverter converter, SettingNameConvention settingNameConvention) : this()
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _settingNameFactory = new SettingNameFactory(settingNameConvention);
        }

        public SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public ISetting Read(SettingName settingName, Type settingType, SettingNameConvention? settingNameConvention)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));

            settingName = _settingNameFactory.CreateSettingName(settingName, settingNameConvention);

            try
            {
                var setting = ReadCore(settingName);

                return
                    setting is null
                        ? default
                        : new Setting
                        (
                            setting.Name,
                            setting.Value is null ? default : _converter.Deserialize(setting.Value, settingType)
                        );
            }
            catch (Exception innerException)
            {
                throw ($"ReadSetting", $"An error occured while trying to read {settingName.ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        [CanBeNull]
        protected abstract ISetting ReadCore(SettingName name);

        public void Write(SettingName settingName, object value, SettingNameConvention? settingNameConvention)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            settingName = _settingNameFactory.CreateSettingName(settingName, settingNameConvention);

            try
            {
                value = value is null ? null : _converter.Serialize(value);
                WriteCore(new Setting(settingName, value));
            }
            catch (Exception innerException)
            {
                throw ($"WriteSetting{nameof(Exception)}", $"Cannot write {settingName.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void WriteCore(ISetting setting);

        private static string CreateDefaultName(Type providerType)
        {
            return providerType.ToPrettyString() + InstanceCounters.AddOrUpdate(providerType.ToPrettyString(), name => 1, (name, counter) => counter + 1);
        }

        protected Exception CreateAmbiguousSettingException(IEnumerable<SoftString> names)
        {
            throw DynamicException.Factory.CreateDynamicException(
                $"AmbiguousSetting{nameof(Exception)}",
                $"Multiple settings found: {names.Select(name => name.ToString()).Join(", ").EncloseWith("[]")}",
                null
            );
        }

        #region IEquatable<ISettingProvider>

        public override int GetHashCode()
        {
            return AutoEquality<ISettingProvider>.Comparer.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            return obj is ISettingProvider other && Equals(other);
        }

        public bool Equals(ISettingProvider other)
        {
            return AutoEquality<ISettingProvider>.Comparer.Equals(this, other);
        }

        #endregion
    }
}