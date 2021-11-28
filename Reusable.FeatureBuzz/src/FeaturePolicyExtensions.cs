using Reusable.FeatureBuzz.Policies;

namespace Reusable.FeatureBuzz
{
    public static class FeaturePolicyExtensions
    {
        public static bool IsEnabled(this IFeaturePolicy policy, FeatureContext context)
        {
            return policy.State(context) switch
            {
                FeatureState.Enabled => true,
                FeatureState.Disabled => false,
                _ => default
            };
        }
        
        public static bool IsEnabled(this IFeaturePolicy policy, IFeatureCollection features, string name, object? parameter)
        {
            return policy.IsEnabled(FeatureContext.Create(features, name, parameter));
        }
        
        public static FeatureState State(this IFeaturePolicy policy, IFeatureCollection features, string name, object? parameter)
        {
            return policy.State(FeatureContext.Create(features, name, parameter));
        }
    }
}