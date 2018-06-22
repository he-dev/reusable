using System.Collections;
using System.Collections.Generic;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class InMemory : SettingProvider, IEnumerable<KeyValuePair<SoftString, object>>
    {
        private readonly IDictionary<SoftString, object> _settings = new Dictionary<SoftString, object>();

        public InMemory(ISettingConverter converter) : base(converter) { }

        protected override ISetting ReadCore(IReadOnlyCollection<SoftString> names)
        {
            foreach (var name in names)
            {
                if (_settings.TryGetValue(name, out var value))
                {
                    return new Setting(name) { Value = value };
                }
            }

            return default;
        }

        protected override void WriteCore(ISetting setting)
        {
            _settings[setting.Name] = setting.Value;
        }

        #region IEnumerable

        public void Add(ISetting setting) => Write(setting);

        public void Add(string name, object value) => Write(new Setting(name) { Value = value });

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