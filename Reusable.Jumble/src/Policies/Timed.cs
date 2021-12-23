using System;
using JetBrains.Annotations;

namespace Reusable.Jumble.Policies;

[PublicAPI]
public class Timed : FeaturePolicy
{
    public Timed(TimeSpan expiration) => Expiration = expiration;

    public TimeSpan Expiration { get; }
    
    public DateTime FirstUse { get; set; }

    public void Reset() => FirstUse = DateTime.MinValue;
    
    public override FeatureState GetState(FeatureStateContext context)
    {
        return (DateTime.UtcNow - FirstUse) < Expiration ? FeatureState.Enabled : FeatureState.Disabled;
    }
}