namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Disables a feature after a single usage.
    /// </summary>
    public class Once : IFeaturePolicy, IFinalizable
    {
        public FeatureState State(FeatureContext context) => FeatureState.Enabled;

        public void Finally(FeatureContext context, FeatureState after)
        {
            switch (after)
            {
                case FeatureState.Enabled:
                    context.Toggle.Remove(context.Feature);
                    break;
            }
        }
    }
}