using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class AppSetting : LogAttachement
    {
        private readonly string _key;

        public AppSetting(string name, string key) : base(name)
        {
            _key = key;
        }

        public override object Compute(Log log)
        {
            return ConfigurationManager.AppSettings[_key];
        }

        public static IEnumerable<AppSetting> FromAppConfig(string prefix, params string[] keys)
        {
            return
                from key in keys
                select new AppSetting(key, $"{prefix}{key}");
        }
    }
}