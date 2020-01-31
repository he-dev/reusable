using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Beaver.Policies;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        /// <summary>
        /// Gets or sets feature. Use 'Feature.Remove' to reset a feature. Throws when trying to set a locked feature.
        /// </summary>
        Feature this[string name] { get; set; }
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IFeatureCollection features;

        public FeatureToggle(IFeaturePolicy fallbackPolicy, IFeatureCollection features)
        {
            this.features = features;
            this.features.Add(new Feature.Fallback { Policy = fallbackPolicy });
        }

        public FeatureToggle(IFeaturePolicy fallbackPolicy) : this(fallbackPolicy, new FeatureCollection()) { }

        public Feature this[string name]
        {
            get => features[name] ?? features[new Feature.Fallback()]!;
            set
            {
                if (this.IsLocked(name)) throw new InvalidOperationException($"Feature '{name}' is locked and cannot be changed.");

                features[name] = value is Feature.Remove ? default : value;
            }
        }
    }
}