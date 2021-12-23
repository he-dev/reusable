using System;
using JetBrains.Annotations;

namespace Reusable.Jumble.Policies;

// Quick-n-dirty policy.
[PublicAPI]
public class AdHoc : FeaturePolicy
{
    public AdHoc(Func<FeatureStateContext, FeatureState> state) => GetStateFunc = state;
    
    private Func<FeatureStateContext, FeatureState> GetStateFunc { get; }

    public override FeatureState GetState(FeatureStateContext context) => GetStateFunc(context);
}