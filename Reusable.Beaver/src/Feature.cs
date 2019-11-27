using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public class Feature : IEquatable<Feature>, IEquatable<string>
    {
        public Feature() => Tags = new HashSet<string>(SoftString.Comparer);

        public Feature(string name) : this() => Name = name;

        public Feature(Feature other) : this(other.Name)
        {
            Tags.UnionWith(other.Tags);
            Description = other.Description;
            Telemetry = other.Telemetry;
            Parameter = other.Parameter;
            Toggle = other.Toggle;
        }

        public string Name { get; }

        public ISet<string> Tags { get; }

        public string? Description { get; set; }

        public bool Telemetry { get; set; }

        public object? Parameter { get; set; }

        public IFeatureToggle? Toggle { get; set; }

        public override string ToString() => Name;

        public override int GetHashCode() => SoftString.Comparer.GetHashCode(this);

        public override bool Equals(object? obj) => Equals((obj as Feature)?.Name);

        public bool Equals(Feature? other) => Equals(other?.Name);

        public bool Equals(string? other) => SoftString.Comparer.Equals(Name, other);

        public static implicit operator Feature(string name) => new Feature(name);

        public static implicit operator Feature((string Name, object? Parameter) feature) => new Feature(feature.Name) { Parameter = feature.Parameter };

        public static implicit operator Feature(Selector selector) => new Feature(selector.ToString());

        public static implicit operator string(Feature feature) => feature.ToString();
    }
}