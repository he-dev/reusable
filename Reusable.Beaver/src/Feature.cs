using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Beaver.Annotations;
using Reusable.Beaver.Policies;

namespace Reusable.Beaver
{
    [Beaver]
    [PublicAPI]
    public class Feature : IEquatable<Feature>, IEquatable<string>
    {
        private IFeaturePolicy _policy;

        public Feature(string name, IFeaturePolicy policy, IEnumerable<string>? tags = default)
        {
            Name = name;
            _policy = policy;
            Tags = new SortedSet<string>(tags ?? Enumerable.Empty<string>(), SoftString.Comparer);
        }

        public string Name { get; }

        public IEnumerable<string> Tags { get; }

        /// <summary>
        /// Gets or sets feature-policy. Throws when feature is locked. Does nothing when locking an already locked feature.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IFeaturePolicy Policy
        {
            get => _policy;
            set
            {
                if (_policy is Lock && !(value is Lock))
                {
                    throw new InvalidOperationException($"Feature '{this}' is locked and cannot be changed.");
                }
                _policy = value;
            }
        }

        public override string ToString() => Name;

        public override int GetHashCode() => SoftString.Comparer.GetHashCode(this);

        public override bool Equals(object? obj) => Equals((obj as Feature)?.Name);

        public bool Equals(Feature? other) => Equals(other?.Name);

        public bool Equals(string? other) => SoftString.Comparer.Equals(Name, other);

        public static implicit operator Feature(string name) => new Feature(name, FeaturePolicy.AlwaysOff);

        public static implicit operator string(Feature feature) => feature.ToString();

        public class Fallback : Feature
        {
            public Fallback(string name, IFeaturePolicy policy) : base(name, policy.Lock()) { }
        }

        public class Telemetry : Feature
        {
            public Telemetry(string name, IFeaturePolicy policy) : base(CreateName(name), policy) { }

            public static string CreateName(string name) => $"{name}@{nameof(Telemetry)}";
        }
    }
}