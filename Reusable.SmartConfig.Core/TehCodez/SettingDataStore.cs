using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingDataStore : IEquatable<ISettingDataStore>
    {
        [AutoEqualityProperty]
        [NotNull]
        SoftString Name { get; }

        [NotNull]
        ISettingNameGenerator SettingNameGenerator { get; }

        [CanBeNull]
        ISetting Read([NotNull] SoftString settingName, Type settingType);

        void Write([NotNull] ISetting setting);
    }

    [PublicAPI]
    public abstract partial class SettingDataStore : ISettingDataStore
    {
        private static volatile int _instanceCounter;

        private readonly ISettingConverter _converter;

        private ISettingNameGenerator _settingNameGenerator;

        private SoftString _name;

        private SettingDataStore()
        {
            Name = CreateDefaultName(GetType());
            SettingNameGenerator = SmartConfig.SettingNameGenerator.Default;
        }

        protected SettingDataStore(ISettingConverter converter) : this()
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
            var names = SettingNameGenerator.GenerateSettingNames(settingName).Select(name => (SoftString)(string)name).ToList();
            try
            {
                var setting = ReadCore(names);
                return new Setting
                {
                    Name = setting.Name,
                    Value = _converter.Deserialize(setting, settingType)
                };
            }
            catch (Exception innerException)
            {
                throw ("SettingReadException", $"Cannot read {names.Last().ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract ISetting ReadCore(IEnumerable<SoftString> names);

        public void Write(ISetting setting)
        {
            try
            {
                WriteCore(new Setting
                {
                    Name = setting.Name,
                    Value = setting.Value.IsNull() ? null : _converter.Serialize(setting.Value)
                });
            }
            catch (Exception innerException)
            {
                throw ("SettingWriteException", $"Cannot write {setting.Name.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void WriteCore(ISetting setting);

        protected static string CreateDefaultName(Type datastoreType)
        {
            return $"{datastoreType.Name}{_instanceCounter++}";
        }
    }

    public abstract partial class SettingDataStore
    {
        public override int GetHashCode() => AutoEquality<ISettingDataStore>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as ISettingDataStore);

        public bool Equals(ISettingDataStore other) => AutoEquality<ISettingDataStore>.Comparer.Equals(this, other);
    }
}