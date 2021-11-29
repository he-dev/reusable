using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.FeatureBuzz.Policies.Abstractions
{
    public abstract class Dependent : FeaturePolicy
    {
        private readonly IFeatureCollection _features;
        private readonly IEnumerable<string> _names;
        private readonly Func<IEnumerable<Feature>, FeatureContext, bool> _predicate;

        protected Dependent(IFeatureCollection features, IEnumerable<string> names, Func<IEnumerable<Feature>, FeatureContext, bool> predicate)
        {
            _features = features;
            _names = names;
            _predicate = predicate;
        }

        public override FeatureState State(FeatureContext context)
        {
            return _predicate(_names.Select(n => _features[n]), context) switch
            {
                true => FeatureState.Enabled,
                false => FeatureState.Disabled
            };
        }
    }
}