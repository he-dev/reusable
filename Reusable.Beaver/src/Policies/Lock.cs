namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Locks a feature preventing further changes. The toggle will throw an exception if a locked feature is set.
    /// </summary>
    public class Lock : IFeaturePolicy, IFinalizable
    {
        private readonly IFeaturePolicy _policy;

        public Lock(IFeaturePolicy policy) => _policy = policy;

        public FeatureState State(FeatureContext context) => _policy.State(context);

        public void Finally(FeatureContext context, FeatureState after) => (_policy as IFinalizable)?.Finally(context, after);
    }
}