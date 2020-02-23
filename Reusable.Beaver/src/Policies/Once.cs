namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Disables a feature after a single usage.
    /// </summary>
    public class Once : FeaturePolicy, IFinalizable
    {
        public override FeatureState State(FeatureContext context) => FeatureState.Enabled;

        public void Finalize(FeatureContext context, FeatureState state)
        {
            if (state == FeatureState.Enabled)
            {
                context.Feature.Policy = AlwaysOff;
            }
        }
    }
}