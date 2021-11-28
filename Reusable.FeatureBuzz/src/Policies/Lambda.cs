using System;
using JetBrains.Annotations;

namespace Reusable.FeatureBuzz.Policies
{
    [PublicAPI]
    public class Lambda : FeaturePolicy, IFeaturePolicyFilter
    {
        private readonly Func<FeatureContext, FeatureState> _state;

        public Lambda(Func<FeatureContext, FeatureState> state) => _state = state;

        public Action<IFeatureCollection , string , object?>? OnFeatureUsed { get; set; }

        public override FeatureState State(FeatureContext context) => _state(context);

        void IFeaturePolicyFilter.OnFeatureUsed(IFeatureCollection features, string name, object? parameter) => OnFeatureUsed?.Invoke(features, name, parameter);
    }
}