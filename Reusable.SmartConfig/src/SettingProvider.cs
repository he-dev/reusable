using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingProvider : IEquatable<ISettingProvider>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftString Name { get; }        

        [CanBeNull]
        ISetting Read([NotNull] SelectQuery query);

        void Write([NotNull] UpdateQuery query);
    }
    
    [PublicAPI]
    public abstract class SettingProvider : ISettingProvider
    {
        private readonly ISettingConverter _converter;

        private ISettingNameFactory _settingNameFactory;

        private SoftString _name;

        private SettingProvider()
        {
            Name = this.CreateDefaultName();
        }

        protected SettingProvider([NotNull] ISettingNameFactory settingNameFactory, [NotNull] ISettingConverter converter)
            : this()
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _settingNameFactory = settingNameFactory ?? throw new ArgumentNullException(nameof(settingNameFactory));
        }

        public SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public ISetting Read(SelectQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var providerNaming= this.Naming(query);
            var providerSettingName = _settingNameFactory.CreateProviderSettingName(query.SettingName, providerNaming);

            try
            {
                var setting = ReadCore(providerSettingName);

                return
                    setting is null
                        ? default
                        : new Setting
                        (
                            setting.Name,
                            setting.Value is null ? default : _converter.Deserialize(setting.Value, query.SettingType)
                        );
            }
            catch (Exception innerException)
            {
                throw ($"ReadSetting", $"An error occured while trying to read {providerSettingName.ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        [CanBeNull]
        protected abstract ISetting ReadCore(SettingName name);

        public void Write(UpdateQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var providerNaming= this.Naming(query);
            var providerSettingName = _settingNameFactory.CreateProviderSettingName(query.SettingName, providerNaming);

            try
            {
                var value = query.Value is null ? null : _converter.Serialize(query.Value);
                WriteCore(new Setting(query.SettingName, value));
            }
            catch (Exception innerException)
            {
                throw ("WriteSetting", $"An error occured while trying to write {providerSettingName.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void WriteCore(ISetting setting);       

        #region IEquatable<ISettingProvider>

        public bool Equals(ISettingProvider other) => AutoEquality<ISettingProvider>.Comparer.Equals(this, other);
        
        public override bool Equals(object obj) => obj is ISettingProvider other && Equals(other);
        
        public override int GetHashCode() => AutoEquality<ISettingProvider>.Comparer.GetHashCode(this);

        #endregion
    }

    public class SettingProviderNaming
    {
        public SettingNameStrength Strength { get; set; }

        public string Prefix { get; set; }

        public PrefixHandling PrefixHandling { get; set; }
    }
}