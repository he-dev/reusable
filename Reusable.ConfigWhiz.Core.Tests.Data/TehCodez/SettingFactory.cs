using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests.Common
{
    public abstract class SettingFactory
    {
        public static IEnumerable<ISetting> ReadSettings()
        {
            var json = ResourceReader.ReadEmbeddedResource<SettingFactory>("Resources.Settings.json");
            var testData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var settings =
                from item in testData
                select new Setting
                {
                    Name = item.Key,
                    Value = item.Value
                };
            return settings;
        }
    }
}
