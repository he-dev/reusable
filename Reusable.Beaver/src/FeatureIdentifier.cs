using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public class FeatureIdentifier : IEquatable<FeatureIdentifier>, IEquatable<string>
    {
        public FeatureIdentifier([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [AutoEqualityProperty]
        public string Name { get; }

        public string Description { get; set; }

        public override string ToString() => Name;

        public override int GetHashCode() => AutoEquality<FeatureIdentifier>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is FeatureIdentifier fn && Equals(fn);

        public bool Equals(FeatureIdentifier featureIdentifier) => AutoEquality<FeatureIdentifier>.Comparer.Equals(this, featureIdentifier);

        public bool Equals(string name) => Equals(this, new FeatureIdentifier(name));

        public static implicit operator FeatureIdentifier(string name) => new FeatureIdentifier(name);

        public static implicit operator FeatureIdentifier(Selector selector) => new FeatureIdentifier(selector.ToString());

        public static implicit operator string(FeatureIdentifier featureIdentifier) => featureIdentifier.ToString();
    }
}