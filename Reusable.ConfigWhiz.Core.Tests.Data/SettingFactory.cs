using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz.Core.Tests.Data
{
    public class SettingFactory
    {
        public static IEnumerable<ISetting> ReadSettings<TConsumer>()
        {
            var json = ResourceReader.ReadEmbeddedResource<TConsumer>("Settings.json");
            var testData = JsonConvert.DeserializeObject<TestData>(json);
            var settings =
                from property in typeof(TestData).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                from s in property.GetValue(testData) as Dictionary<string, string>
                select new Setting
                {
                    Path = SettingPath.Parse($"{typeof(TConsumer).Namespace}.{typeof(TConsumer).Name}.{property.Name}.{s.Key}"),
                    Value = s.Value
                };
            return settings;
        }

        private class TestData
        {
            public Dictionary<string, string> Numeric { get; set; }
            public Dictionary<string, string> Literal { get; set; }
            public Dictionary<string, string> Other { get; set; }
            public Dictionary<string, string> Paint { get; set; }
            public Dictionary<string, string> Collection { get; set; }
        }
    }
}
