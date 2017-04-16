using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz.Datastores
{
    public class Memory : Datastore, IEnumerable<ISetting>
    {
        public Memory(object handle)
            : base(handle, new[] { typeof(string) })
        { }

        protected Memory(object handle, IEnumerable<Type> supportedTypes)
            : base(handle, supportedTypes)
        { }

        public override Result<IEnumerable<ISetting>> Read(SettingPath settingPath)
        {
            var name = settingPath.ToFullWeakString();
            return
                (from x in Data
                 where x.Path.ToFullWeakString().Equals(name, StringComparison.OrdinalIgnoreCase)
                 select x).ToList();
        }

        public override Result Write(IGrouping<SettingPath, ISetting> settings)
        {
            var name = settings.Key.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance);
            var obsoleteSettings =
                (from x in Data
                 where x.Path.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance).Equals(name, StringComparison.OrdinalIgnoreCase)
                 select x).ToList();
            obsoleteSettings.ForEach(x => Data.Remove(x));

            foreach (var setting in settings) Add(setting);

            return Result.Ok();
        }

        public List<ISetting> Data { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = new List<ISetting>();

        #region IEnumerable

        public void Add(ISetting setting) => Data.Add(setting);

        public void Add(string name, object value) => Data.Add(new Setting
        {
            Path = SettingPath.Parse(name),
            Value = value
        });

        public IEnumerator<ISetting> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}