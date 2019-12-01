using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;

namespace Reusable.Translucent
{
    public class ComplexName : IEnumerable<string>, IEquatable<ComplexName>
    {
        private readonly ISet<string> _secondary;

        public ComplexName() => _secondary = new SortedSet<string>(SoftString.Comparer);

        public ComplexName(string primary) : this() => Primary = primary;
        
        public static ComplexName Empty => new ComplexName();

        public string Primary { get; } = string.Empty;

        public IEnumerable<string> Secondary => _secondary;

        public void Add(string tag) => _secondary.Add(tag);

        public IEnumerator<string> GetEnumerator() => _secondary.Prepend(Primary).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(ComplexName? other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            return
                SoftString.IsNullOrEmpty(Primary) || SoftString.IsNullOrEmpty(other.Primary)
                    ? (_secondary.Empty() && other._secondary.Empty()) || _secondary.Overlaps(other._secondary)
                    : SoftString.Comparer.Equals(Primary, other.Primary);
        }

        public override bool Equals(object? obj) => Equals(obj as ComplexName);

        public override int GetHashCode() => 0;
    }
}