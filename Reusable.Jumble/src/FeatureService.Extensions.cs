using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Jumble;

[PublicAPI]
public static partial class FeatureServiceExtensions
{
    public static bool Contains(this IFeatureService features, IFeatureIdentifier id)
    {
        return features.TryGet(id, out _);
    }

    public static bool TryUpdatePolicy(this IFeatureService features, IFeatureIdentifier id, IFeaturePolicy newPolicy)
    {
        if (features.TryGet(id, out var feature) && features.Usage.CanChangePolicy(new UpdatePolicyContext(features, feature.Id, feature.Policy, newPolicy)))
        {
            feature.Policy = newPolicy;
            return true;
        }

        return false;
    }

    public static FeatureState GetState(this IFeatureService features, IFeatureIdentifier id, object? parameter = default)
    {
        // Get state either from an existing feature or the fallback one.
        var context = new FeatureStateContext(features, id, parameter);
        return
            features.TryGet(id, out var feature)
                ? feature.Policy.GetState(context)
                : features.Usage.GetFallbackPolicy(new FallbackPolicyContext(features, id, parameter)).GetState(context);
    }

    public static bool IsEnabled(this IFeatureService features, IFeatureIdentifier id, object? parameter = default)
    {
        return features.GetState(id, parameter) == FeatureState.Enabled;
    }
    
    public static bool IsDisabled(this IFeatureService features, IFeatureIdentifier id, object? parameter = default)
    {
        return features.GetState(id, parameter) == FeatureState.Disabled;
    }

    public static bool TryEnable(this IFeatureService features, IFeatureIdentifier id)
    {
        return features.TryUpdatePolicy(id, FeaturePolicy.AlwaysEnabled);
    }

    public static bool TryDisable(this IFeatureService features, IFeatureIdentifier id)
    {
        return features.TryUpdatePolicy(id, FeaturePolicy.AlwaysDisabled);
    }

    public static IFeatureService EnableTelemetry(this IFeatureService features, ILogger<Feature.Telemetry> logger)
    {
        return features.Also(f => f.TryAdd(new Feature.Telemetry(logger)));
    }

    public static IFeatureService DisableTelemetry(this IFeatureService features)
    {
        return features.Also(f => f.TryRemove(FeatureName.From(nameof(Feature.Telemetry)), out _));
    }

    public static IEnumerable<Feature> WhereAttributesOverlap(this IEnumerable<Feature> features, IFeatureAttributeSet attributes)
    {
        return features.Where(f => f.Attributes.OverlapsWith(attributes));
    }
    
    public static IEnumerable<Feature> WhereAttributesIntersect(this IEnumerable<Feature> features, IFeatureAttributeSet attributes)
    {
        return features.Where(f => f.Attributes.SupersetOf(attributes));
    }

    public static IEnumerable<Feature> WhereAttributesExcept(this IEnumerable<Feature> features, IFeatureAttributeSet attributes)
    {
        return features.Where(f => f.Attributes.OverlapsWith(attributes) == false);
    }

    #region IEnumerable

    public static void Add(this IFeatureService features, IFeatureIdentifier id, IFeaturePolicy policy)
    {
        features.TryAdd(new Feature(id, policy));
    }

    #endregion
}