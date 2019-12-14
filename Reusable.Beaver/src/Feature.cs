using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public class Feature : IEquatable<Feature>, IEquatable<string>
    {
        private Feature() => Tags = new HashSet<string>(SoftString.Comparer);

        public Feature(string name) : this() => Name = name;

        public string Name { get; }

        public ISet<string> Tags { get; }

        public string? Description { get; set; }

        public override string ToString() => Name;

        public override int GetHashCode() => SoftString.Comparer.GetHashCode(this);

        public override bool Equals(object? obj) => Equals((obj as Feature)?.Name);

        public bool Equals(Feature? other) => Equals(other?.Name);

        public bool Equals(string? other) => SoftString.Comparer.Equals(Name, other);

        public static implicit operator Feature(string name) => new Feature(name);

        public static implicit operator Feature(Selector selector) => new Feature(selector.ToString());

        public static implicit operator string(Feature feature) => feature.ToString();
    }
}