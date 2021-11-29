using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.FeatureBuzz.Annotations;
using Reusable.FeatureBuzz.Policies;

namespace Reusable.FeatureBuzz
{
    [FeatureBuzz]
    [PublicAPI]
    public class Feature : IEquatable<Feature>, IEquatable<string>
    {
        private IFeaturePolicy _policy;

        public Feature(string name, IFeaturePolicy policy, IImmutableSet<string>? tags = default)
        {
            Name = name;
            _policy = policy;
            Tags = tags ?? ImmutableHashSet<string>.Empty;
        }

        public bool AllowPolicyChange { get; set; } = true;

        public Action<string, IFeaturePolicy, IFeaturePolicy>? OnPolicyChange { get; set; }

        public string Name { get; }

        public IImmutableSet<string> Tags { get; }

        /// <summary>
        /// Gets or sets feature-policy. Throws when feature is locked. Does nothing when locking an already locked feature.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IFeaturePolicy Policy
        {
            get => _policy;
            set
            {
                if (AllowPolicyChange)
                {
                    _policy.Also(previous =>
                    {
                        _policy = value;
                        OnPolicyChange?.Invoke(Name, previous, value);
                    });
                }
            }
        }

        public override string ToString() => Name;

        public override int GetHashCode() => SoftString.Comparer.GetHashCode(this);

        public override bool Equals(object? obj) => Equals((obj as Feature)?.Name);

        public bool Equals(Feature? other) => Equals(other?.Name);

        public bool Equals(string? other) => SoftString.Comparer.Equals(Name, other);

        public static implicit operator Feature(string name) => new Feature(name, FeaturePolicy.Disabled);

        public static implicit operator string(Feature feature) => feature.ToString();

        public class Fallback : Feature
        {
            public Fallback(string name, IFeaturePolicy policy) : base(name, policy) { }
        }

        public class Telemetry : Feature
        {
            public Telemetry(string name, IFeaturePolicy policy) : base(CreateName(name), policy) { }

            public static string CreateName(string name) => $"{name}@{nameof(Telemetry)}";
        }
    }
}