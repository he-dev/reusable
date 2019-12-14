using System;
using System.Linq;
using System.Linq.Custom;
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
        /// Gets or sets feature policy. Use 'Remove' policy to reset a feature. Throws when trying to set policy for a locked feature.
        /// </summary>
        IFeaturePolicy this[Feature feature] { get; set; }
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IContainer<Feature, IFeaturePolicy> _policies;

        public FeatureToggle(IContainer<Feature, IFeaturePolicy> policies) => _policies = policies;

        public FeatureToggle(IFeaturePolicy fallback) : this(new FeaturePolicyContainer(fallback)) { }

        public IFeaturePolicy this[Feature feature]
        {
            get => _policies.GetItem(feature).SingleOrDefault() ?? throw DynamicException.Create($"FeaturePolicyNotFound", $"Could not find policy for '{feature}'.");
            set
            {
                if (this.IsLocked(feature)) throw new InvalidOperationException($"Feature '{feature}' is locked and cannot be changed.");

                switch (value)
                {
                    case Remove _:
                        _policies.RemoveItem(feature);
                        break;
                    default:
                        _policies.AddOrUpdateItem(feature, value);
                        break;
                }
            }
        }
    }
}