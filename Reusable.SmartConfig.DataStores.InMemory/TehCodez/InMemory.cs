using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Collections;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class InMemory : SettingDataStore, IEnumerable<ISetting>
    {
        public InMemory() : base(Enumerable.Empty<Type>()) { }

        public InMemory(IEnumerable<ISetting> settings) : base(Enumerable.Empty<Type>())
        {
            AddRange(settings);
        }

        protected override ISetting ReadCore(IEnumerable<SoftString> names)
        {
            var setting =
                (from name in names
                 from x in Data
                 where x.Name.Equals(name)
                 select x).FirstOrDefault();
            return setting;
        }

        protected override void WriteCore(ISetting setting)
        {
            var current = Data.Single(x => x.Name.Equals(setting.Name));
            current.Value = setting.Value;            
        }

        public List<ISetting> Data { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = new List<ISetting>();

        #region IEnumerable

        public void Add(ISetting setting) => Data.Add(setting);

        public void Add(string name, object value) => Data.Add(new Setting
        {
            Name = name,
            Value = value
        });

        public InMemory AddRange(IEnumerable<ISetting> settings)
        {
            foreach (var setting in settings) Add(setting);
            return this;
        }

        public IEnumerator<ISetting> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}