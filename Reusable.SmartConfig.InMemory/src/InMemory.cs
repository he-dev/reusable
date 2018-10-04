using System.Collections;
using System.Collections.Generic;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class InMemory : SettingProvider, IEnumerable<KeyValuePair<SoftString, object>>
    {
        private readonly IDictionary<SoftString, object> _settings = new Dictionary<SoftString, object>();

        public InMemory(ISettingConverter converter, SettingNameConvention settingNameConvention) : base(converter, settingNameConvention) { }

        protected override ISetting ReadCore(SettingName name)
        {
            return _settings.TryGetValue(name, out var value) ? new Setting(name, value) : default;
        }

        protected override void WriteCore(ISetting setting)
        {
            _settings[setting.Name] = setting.Value;
        }

    #region IEnumerable

        public void Add(ISetting setting) => WriteCore(setting);

        public void Add(string name, object value) => WriteCore(new Setting(SettingName.Parse(name), value));

        public InMemory AddRange(IEnumerable<ISetting> settings)
        {
            foreach (var setting in settings)
            {
                Add(setting);
            }
            return this;
        }

        public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
    }
}