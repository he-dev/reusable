using System;
using JetBrains.Annotations;
using Reusable.Essentials.Collections;

namespace Reusable.Jumble;

public interface IFeatureIdentifier : IEquatable<IFeatureIdentifier> { }

[PublicAPI]
public class FeatureName : IFeatureIdentifier
{
    public FeatureName(string value) => Value = value;

    [AutoEqualityProperty(StringComparison.OrdinalIgnoreCase)]
    public string Value { get; }

    public static IFeatureIdentifier From(string name) => new FeatureName(name);

    public override int GetHashCode() => AutoEquality<string>.Comparer.GetHashCode(Value);

    public override bool Equals(object? obj) => Equals(obj as FeatureName);

    public override string ToString() => Value;

    public bool Equals(IFeatureIdentifier? other) => other is FeatureName name && AutoEquality<string>.Comparer.Equals(name.Value, Value);
}


