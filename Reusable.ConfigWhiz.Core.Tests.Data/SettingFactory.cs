using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz.Tests.Common
{
    public class SettingFactory
    {
        public static IEnumerable<IEntity> ReadSettings()
        {
            var json = ResourceReader.ReadEmbeddedResource<SettingFactory>("Settings.json");
            var testData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var settings =
                from item in testData
                select new Entity
                {
                    Id = Identifier.Parse(item.Key), //$"{typeof(T).Namespace}.{typeof(T).Name}.{property.Name}.{s.Key}"),
                    Value = item.Value
                };
            return settings;
        }
    }
}
