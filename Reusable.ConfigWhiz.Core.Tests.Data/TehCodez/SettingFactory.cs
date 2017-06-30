using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz.Tests.Common
{
    public abstract class SettingFactory
    {
        public static IEnumerable<IEntity> ReadSettings()
        {
            var json = ResourceReader.ReadEmbeddedResource<SettingFactory>("Resources.Settings.json");
            var testData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var settings =
                from item in testData
                select new Entity
                {
                    Id = Identifier.Parse(item.Key),
                    Value = item.Value
                };
            return settings;
        }
    }
}
