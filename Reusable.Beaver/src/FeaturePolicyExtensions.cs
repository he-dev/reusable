using Reusable.Beaver.Policies;

namespace Reusable.Beaver
{
    public static class FeaturePolicyExtensions
    {
        public static Lock Lock(this IFeaturePolicy policy) => new Lock(policy);
    }
}