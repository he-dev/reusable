using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Colin.Collections
{
    public class ImmutableNameSet : IImmutableSet<string>
    {
        private readonly IImmutableSet<string> _names;

        private ImmutableNameSet(params string[] names) => _names = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, names);

        public static readonly ImmutableNameSet Empty = Create(string.Empty);

        public static ImmutableNameSet Create(IEnumerable<string> values) => Create(values.ToArray());

        public static ImmutableNameSet Create(params string[] names) => new ImmutableNameSet(names);

        #region IImmutableSet<string>

        public int Count => _names.Count;
        public IImmutableSet<string> Clear() => _names.Clear();
        public bool Contains(string value) => _names.Contains(value);
        public IImmutableSet<string> Add(string value) => _names.Add(value);
        public IImmutableSet<string> Remove(string value) => _names.Remove(value);
        public bool TryGetValue(string equalValue, out string actualValue) => _names.TryGetValue(equalValue, out actualValue);
        public IImmutableSet<string> Intersect(IEnumerable<string> other) => _names.Intersect(other);
        public IImmutableSet<string> Except(IEnumerable<string> other) => _names.Except(other);
        public IImmutableSet<string> SymmetricExcept(IEnumerable<string> other) => _names.SymmetricExcept(other);
        public IImmutableSet<string> Union(IEnumerable<string> other) => _names.Union(other);
        public bool SetEquals(IEnumerable<string> other) => _names.SetEquals(other);
        public bool IsProperSubsetOf(IEnumerable<string> other) => _names.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<string> other) => _names.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<string> other) => _names.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<string> other) => _names.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<string> other) => _names.Overlaps(other);
        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _names.GetEnumerator();

        #endregion

        //public static bool operator ==(ImmutableNameSet left, ImmutableNameSet right) => left.Overlaps(right);
        //public static bool operator !=(ImmutableNameSet left, ImmutableNameSet right) => !(left == right);

        private sealed class NamesEqualityComparer : IEqualityComparer<ImmutableNameSet>
        {
            public bool Equals(ImmutableNameSet x, ImmutableNameSet y)
            {
                return
                    !ReferenceEquals(x, null) &&
                    !ReferenceEquals(y, null) &&
                    x.Overlaps(y);
            }

            public int GetHashCode(ImmutableNameSet obj) => (obj._names != null ? obj._names.GetHashCode() : 0);
        }

        public static IEqualityComparer<ImmutableNameSet> Comparer { get; } = new NamesEqualityComparer();
    }
}
