using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;
using Reusable.Beaver.Json;
using Reusable.Beaver.Policies;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Utilities.JsonNet;

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
        private readonly IContainer<Feature, IFeaturePolicy> policies;
        private readonly string fallback;

        public FeatureToggle(IContainer<Feature, IFeaturePolicy> policies, string fallback = "")
        {
            this.policies = policies;
            this.fallback = fallback;
        }

        public FeatureToggle(IFeaturePolicy fallback) : this(new FeaturePolicyContainer
        {
            { FeaturePolicy.Fallback, fallback }
        }) { }

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

        public static IFeatureToggle FromJson(string json, IEnumerable<Type>? policies = default)
        {
            var types = TypeDictionary.From(FeaturePolicy.BuiltIn.Concat(policies ?? Enumerable.Empty<Type>()));
            var prettyJsonSerializer = new PrettyJsonSerializer(new DefaultContractResolver(), serializer =>
            {
                serializer.Converters.Add(new FeatureConverter());
                serializer.Converters.Add(new FeaturePolicyConverter());
            });
            var config = prettyJsonSerializer.Deserialize<FeatureConfiguration>(json, types);
            var features = new FeaturePolicyContainer();

            foreach (var setting in config.Settings)
            {
                features.Add(setting.Feature, setting.Policy);
            }

            return new FeatureToggle(features, config.Fallback);
        }
    }
}