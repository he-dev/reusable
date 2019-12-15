using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    public class FeatureConfiguration
    {
        public List<FeatureSetting> Settings { get; set; }

        public string Fallback { get; set; } = string.Empty;
    }

    [UsedImplicitly]
    public class FeatureSetting
    {
        public Feature Feature { get; set; }

        public IFeaturePolicy Policy { get; set; }
    }
}