using System.Linq;

namespace Reusable.Beaver
{
    public partial class FeatureToggle
    {
        public static IFeatureToggle FromConfiguration(FeatureConfiguration first, FeatureConfiguration? second = default)
        {
            var diff =
                from f in first.Settings
                join s in second.Settings on f.Feature.Name equals s.Feature.Name into o
                from s in o.DefaultIfEmpty()
                select (f, s);

            var features = new FeaturePolicyContainer();

            foreach (var (f, s) in diff)
            {
                if (s is {})
                {
                    features.Add(s.Feature, s.Policy);
                }
                else
                {
                    features.Add(f.Feature, f.Policy);
                }
            }

            return new FeatureToggle(features, second?.Fallback ?? first.Fallback);
        }
    }
}