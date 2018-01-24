using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.ThirdParty.JsonNetUtilities;

namespace Reusable.SmartConfig.DataStores
{
    public static class JsonSettingConverterFactory
    {
        public static JsonSettingConverter CreateDefault()
        {
            return new JsonSettingConverter
            {
                Settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters =
                    {
                        new StringEnumConverter(),
                        new JsonColorConverter()
                    }
                },
                StringTypes = new HashSet<Type>
                {
                    typeof(string),
                    typeof(Enum),
                    typeof(TimeSpan),
                    typeof(DateTime),
                    typeof(Color)
                },
            };
        }
    }
}