namespace Reusable.Beaver.Policies
{
    public class Lock : IFeaturePolicy, IFinalizable
    {
        private readonly IFeaturePolicy _policy;
        public Lock(IFeaturePolicy policy) => _policy = policy;
        public Feature Feature => _policy.Feature;
        public bool IsEnabled(Feature feature) => _policy.IsEnabled(feature);
        public void FinallyMain(Feature feature) => (_policy as IFinalizable)?.FinallyMain(feature);
        public void FinallyFallback(Feature feature) => (_policy as IFinalizable)?.FinallyFallback(feature);
        public void FinallyIIf(Feature feature) => (_policy as IFinalizable)?.FinallyIIf(feature);
    }
}