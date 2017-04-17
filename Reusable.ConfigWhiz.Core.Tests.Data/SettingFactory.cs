using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz.Core.Tests.Data
{
    public class SettingFactory
    {
        private static IEnumerable<ISetting> ReadSettings<TNamespaceProvider>()
        {
            var json = ResourceReader.ReadEmbeddedResource<SettingFactory>("Settings.json");
            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return 
                from s in settings
                select  new Setting
                {
                    Path = SettingPath.Parse($"{typeof(TNamespaceProvider).Namespace}.{s.Key}"),
                    Value = s.Value
                };
        }
    }
}
