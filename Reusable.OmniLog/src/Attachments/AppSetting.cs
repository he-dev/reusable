using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Attachments
{
    public class AppSetting : LogAttachment
    {
        private readonly string _key;

        public AppSetting(string name, string key) : base(name)
        {
            _key = key;
        }

        public override object Compute(ILog log)
        {
            return ConfigurationManager.AppSettings[_key];
        }

        public static IEnumerable<AppSetting> CreateMany(string prefix, params string[] keys)
        {
            return
                from key in keys
                select new AppSetting(key, $"{prefix}{key}");
        }        
    }
}