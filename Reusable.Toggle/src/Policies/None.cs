using System.Collections.Generic;
using System.Linq;
using Reusable.FeatureBuzz.Policies.Abstractions;

namespace Reusable.FeatureBuzz.Policies
{
    public class None : Dependent
    {
        public None(IFeatureCollection features, IEnumerable<string> names) : base(features, names, Predicate) { }

        private static bool Predicate(IEnumerable<Feature> features, FeatureContext context)
        {
            return features.All(f => FeaturePolicyExtensions.IsEnabled(f.Policy, context) == false);
        }
    }
}