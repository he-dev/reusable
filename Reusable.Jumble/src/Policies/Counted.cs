using JetBrains.Annotations;

namespace Reusable.Jumble.Policies;

[PublicAPI]
public class Counted : FeaturePolicy
{
    public Counted(long maxCount) => MaxCount = maxCount;

    public long MaxCount { get; }

    public long UseCount { get; set; }

    public void Reset() => UseCount = 0;

    public override FeatureState GetState(FeatureStateContext context)
    {
        return UseCount < MaxCount ? FeatureState.Enabled : FeatureState.Disabled;
    }
}