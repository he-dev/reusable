using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Toggle.Mechanics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Toggle;

[PublicAPI]
public interface IFeatureService : IEnumerable<(string Name, FeatureMechanics Mehanics)>
{
    FeatureMechanics this[string name] { get; set; }

    bool CanUse(string name);

    bool TryUse(string name, [MaybeNullWhen(false)] out FeatureScope scope);
}

[PublicAPI]
public class FeatureService : IFeatureService
{
    private Dictionary<string, FeatureMechanics> Features { get; } = new(SoftString.Comparer);

    public FeatureMechanics this[string name]
    {
        get => Features.GetItemSafely(name);
        set => Features[name] = value;
    }

    public IEnumerator<(string Name, FeatureMechanics Mehanics)> GetEnumerator() => Features.Select(x => (x.Key, x.Value)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(string name, FeatureMechanics mechanics) => Features[name] = mechanics;

    public bool CanUse(string name) => this[name].CanUse(this);

    public virtual bool TryUse(string name, [MaybeNullWhen(false)] out FeatureScope scope)
    {
        if (CanUse(name))
        {
            scope = this[name].BeginUnitOfWork(this);
            return true;
        }

        scope = default;
        return false;
    }
}

[PublicAPI]
public class FeatureServiceWithTelemetry : FeatureService
{
    public FeatureServiceWithTelemetry(ILoggerFactory loggerFactory) => LoggerFactory = loggerFactory;

    private ILoggerFactory LoggerFactory { get; }

    public override bool TryUse(string name, [MaybeNullWhen(false)] out FeatureScope scope)
    {
        if (base.TryUse(name, out var inner))
        {
            var logger = LoggerFactory.CreateLogger(name);
            var unitOfWork = logger.BeginUnitOfWork(name);
            scope = new FeatureScope(() =>
            {
                inner.Dispose();
                unitOfWork.Dispose();
            });

            return true;
        }

        scope = default;
        return false;
    }
}

public static class Test
{
    public static void Example()
    {
        var features = new FeatureService
        {
            { "Test", new CountdownFeature(1) }
        };

        if (features.TryUse("test", out var scope))
        {
            using var s = scope;
            // do something...
        }
    }
}