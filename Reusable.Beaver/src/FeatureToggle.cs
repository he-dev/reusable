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
        /// Gets or sets feature policy. Use 'Remove' policy to reset a feature. Throws when trying to set policy for a locked feature.
        /// </summary>
        Feature this[string name] { get; set; }
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IContainer<string, Feature> policies;
        private readonly Feature fallback;

        public FeatureToggle(IContainer<string, Feature> policies, IFeaturePolicy fallbackPolicy)
        {
            this.policies = policies;
            this.fallback = new Feature.Fallback { Policy = fallbackPolicy };
        }

        public FeatureToggle(IFeaturePolicy fallbackPolicy) : this(new FeaturePolicyContainer(), fallbackPolicy) { }

        public Feature this[string name]
        {
            get => policies.GetItem(name).SingleOrDefault() ?? fallback;
            set
            {
                if (this.IsLocked(name)) throw new InvalidOperationException($"Feature '{name}' is locked and cannot be changed.");

                switch (value.Policy)
                {
                    case Remove _:
                        policies.RemoveItem(name);
                        break;
                    default:
                        policies.AddOrUpdateItem(name, value);
                        break;
                }
            }
        }
    }
}