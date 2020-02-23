using System;

namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Allows to specify a delegate that determines whether a feature can be used.
    /// </summary>
    public class Lambda : FeaturePolicy, IFinalizable
    {
        private readonly Func<FeatureContext, FeatureState> _state;
        private readonly Action<FeatureContext, FeatureState>? _finalize;

        public Lambda(Func<FeatureContext, FeatureState> state, Action<FeatureContext, FeatureState>? finalize = default)
        {
            _state = state;
            _finalize = finalize;
        }

        public override FeatureState State(FeatureContext context) => _state(context);

        public void Finalize(FeatureContext context, FeatureState state) => _finalize?.Invoke(context, state);
    }
}