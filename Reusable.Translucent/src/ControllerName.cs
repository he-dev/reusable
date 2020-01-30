using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Translucent
{
    [PublicAPI]
    public class ControllerName : IEquatable<ControllerName>
    {
        public ControllerName(string value, params string[] tags)
        {
            Value = value;
            Tags = new SortedSet<string>(tags ?? Enumerable.Empty<string>(), SoftString.Comparer);
        }

        public static ControllerName Empty => new ControllerName(string.Empty);

        [AutoEqualityProperty]
        public string Value { get; }

        public ISet<string> Tags { get; }

        public bool Equals(ControllerName? other) => AutoEquality<ControllerName>.Comparer.Equals(this, other);

        public override bool Equals(object? obj) => Equals(obj as ControllerName);

        public override int GetHashCode() => AutoEquality<ControllerName>.Comparer.GetHashCode(this);

        public override string ToString() => Value;

        public static implicit operator ControllerName(string value) => new ControllerName(value);

        public static implicit operator string(ControllerName name) => name?.ToString();
    }
}