using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public class FeatureIdentifier : IEquatable<FeatureIdentifier>, IEquatable<string>
    {
        public FeatureIdentifier(string name) => Name = name;

        public string Name { get; }

        public string Description { get; set; }

        public override string ToString() => Name;

        public override int GetHashCode() => SoftString.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as FeatureIdentifier);

        public bool Equals(FeatureIdentifier featureIdentifier) => SoftString.Comparer.Equals(this, featureIdentifier);

        public bool Equals(string name) => Equals(this, new FeatureIdentifier(name));

        public static implicit operator FeatureIdentifier(string name) => new FeatureIdentifier(name);

        public static implicit operator FeatureIdentifier(Selector selector) => new FeatureIdentifier(selector.ToString());

        public static implicit operator string(FeatureIdentifier featureIdentifier) => featureIdentifier.ToString();
    }
}