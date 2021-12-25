using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Toggle;

public record FallbackPolicyContext(IFeatureService Features, IFeatureIdentifier Id, object? Parameter = default);

public record UpdatePolicyContext(IFeatureService Features, IFeatureIdentifier Id, IFeaturePolicy CurrentPolicy, IFeaturePolicy NewPolicy);

public record FeatureStateContext(IFeatureService Features, IFeatureIdentifier Id, object? Parameter);

public record FeatureUsageContext(IFeatureService Features, IFeatureIdentifier Id, object? Parameter, Exception? Exception);

[PublicAPI]
public interface IFeatureService : IEnumerable<Feature>
{
    IFeatureUsage Usage { get; }

    bool TryAdd(Feature feature);

    bool TryGet(IFeatureIdentifier id, out Feature feature);

    bool TryRemove(IFeatureIdentifier id, out Feature feature);
}

[PublicAPI]
public class FeatureService : IFeatureService
{
    private ConcurrentDictionary<IFeatureIdentifier, Feature> Features { get; } = new();

    public FeatureService(IFeatureUsage usage) => Usage = usage;

    public FeatureService() : this(new DefaultUsage(FeaturePolicy.AlwaysEnabled)) { }

    public static IFeatureService Empty() => new FeatureService();

    public IFeatureUsage Usage { get; }

    public bool TryAdd(Feature feature) => Features.TryAdd(feature.Id, feature);

    public bool TryGet(IFeatureIdentifier id, out Feature feature) => Features.TryGetValue(id, out feature!);

    public bool TryRemove(IFeatureIdentifier id, out Feature feature) => Features.TryRemove(id, out feature!);

    public IEnumerator<Feature> GetEnumerator() => Features.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}