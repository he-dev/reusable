using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Beaver.Json;
using Reusable.Utilities.JsonNet;

namespace Reusable.Beaver
{
    [UsedImplicitly]
    public class FeatureConfiguration
    {
        [JsonRequired]
        public List<FeatureSetting> Settings { get; set; } = default!;

        public string Fallback { get; set; } = string.Empty;

        public static FeatureConfiguration FromJson(string json, IEnumerable<Type>? policies = default)
        {
            var types = TypeDictionary.From(FeaturePolicy.BuiltIn.Concat(policies ?? Enumerable.Empty<Type>()));
            var prettyJsonSerializer = new PrettyJsonSerializer(new DefaultContractResolver(), serializer =>
            {
                serializer.Converters.Add(new FeatureConverter());
                serializer.Converters.Add(new FeaturePolicyConverter());
            });
            return prettyJsonSerializer.Deserialize<FeatureConfiguration>(json, types);
        }
    }

    [UsedImplicitly]
    public class FeatureSetting
    {
        public Feature Feature { get; set; }

        public IFeaturePolicy Policy { get; set; }
    }
}