using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data.Repositories;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests.Common
{
    public abstract class TestSettingRepository
    {
        public static IEnumerable<ISetting> Settings
        {
            get
            {
                var json = ResourceReader.Default.FindString(name => name.Contains("Settings"));
                var testData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return
                    from item in testData
                    select new Setting
                    {
                        Name = item.Key,
                        Value = item.Value
                    };
            }
        }
    }
}
