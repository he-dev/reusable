using Reusable.Beaver.Policies;

namespace Reusable.Beaver
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
        
        public static Lock Lock(this IFeaturePolicy policy) => new Lock(policy);
    }
}