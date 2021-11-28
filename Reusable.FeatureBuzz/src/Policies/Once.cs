using System.Collections.Generic;
using System.Linq;

namespace Reusable.FeatureBuzz.Policies
{
    /// <summary>
    /// Disables a feature after a single usage.
    /// </summary>
    public class Once : FeaturePolicy, IFeaturePolicyFilter
    {
        public override FeatureState State(FeatureContext context) => FeatureState.Enabled;

        public void OnFeatureUsed(IFeatureCollection features, string name, object? parameter)
        {
            features[name].Policy = Disabled;
        }
    }
}