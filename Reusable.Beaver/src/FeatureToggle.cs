using System;
using System.Linq;
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

    public partial class FeatureToggle : IFeatureToggle
    {
        private readonly IContainer<Feature, IFeaturePolicy> policies;
        private readonly string fallback;

        public FeatureToggle(IContainer<Feature, IFeaturePolicy> policies, string fallback = "")
        {
            this.policies = policies;
            this.fallback = fallback;
        }

        public FeatureToggle(IFeaturePolicy fallback) : this(new FeaturePolicyContainer { { FeaturePolicy.Fallback, fallback } }) { }

        public IFeaturePolicy this[Feature feature]
        {
            get
            {
                return
                    policies.GetItem(feature).SingleOrDefault() ??
                    policies.GetItem(fallback).SingleOrDefault() ?? throw DynamicException.Create($"FeaturePolicyNotFound", $"Could not find policy for '{feature}'.");
            }
            set
            {
                if (this.IsLocked(feature)) throw new InvalidOperationException($"Feature '{feature}' is locked and cannot be changed.");

                switch (value)
                {
                    case Remove _:
                        policies.RemoveItem(feature);
                        break;
                    default:
                        policies.AddOrUpdateItem(feature, value);
                        break;
                }
            }
        }
    }
}