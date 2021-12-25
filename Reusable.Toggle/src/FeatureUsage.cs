using System;
using JetBrains.Annotations;
using Reusable.Toggle.Policies;

namespace Reusable.Toggle;

[PublicAPI]
public interface IFeatureUsage
{
    IFeaturePolicy GetFallbackPolicy(FallbackPolicyContext context);

    bool CanChangePolicy(UpdatePolicyContext context);

    void AfterUse(FeatureUsageContext context);
}

[PublicAPI]
public class DefaultUsage : IFeatureUsage
{
    public DefaultUsage(IFeaturePolicy fallbackPolicy) => FallbackPolicy = fallbackPolicy;

    private IFeaturePolicy FallbackPolicy { get; }

    public virtual IFeaturePolicy GetFallbackPolicy(FallbackPolicyContext context) => FallbackPolicy;

    public virtual bool CanChangePolicy(UpdatePolicyContext context) => true;

    public virtual void AfterUse(FeatureUsageContext context)
    {
        if (context.Features.TryGet(context.Id, out var feature))
        {
            // Don't count indefinitely.
            if(feature.Policy is Counted counted && counted.UseCount < counted.MaxCount)
            {
                counted.UseCount++;
            }

            if (feature.Policy is Timed timed && timed.FirstUse == DateTime.MinValue)
            {
                timed.FirstUse = DateTime.UtcNow;
            }
        }
    }
}