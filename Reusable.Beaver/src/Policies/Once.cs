namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Disables a feature after a single usage.
    /// </summary>
    public class Once : FeaturePolicy, IFinalizable
    {
        public override FeatureState State(FeatureContext context) => FeatureState.Enabled;

        public void Finally(FeatureContext context, FeatureState after)
        {
            switch (after)
            {
                case FeatureState.Enabled:
                    context.Toggle.TryRemove(context.Feature.Name, out _);
                    break;
            }
        }
    }
}