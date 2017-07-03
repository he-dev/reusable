using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine.Collections
{
    public interface IImmutableNameSet : IImmutableSet<string>, IEquatable<IImmutableSet<string>>, IEquatable<IImmutableNameSet> { }

    /// <summary>
    /// Name set used for command and argument names.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ImmutableNameSet : IImmutableNameSet
    {
        private readonly IImmutableSet<string> _names;

        private ImmutableNameSet(params string[] names) => _names = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, names);

        [NotNull]
        public static readonly IImmutableNameSet Empty = Create();

        //[NotNull]
        //public static readonly IImmutableNameSet DefaultCommandName = Create("Default");

        [NotNull]
        public static IEqualityComparer<IImmutableSet<string>> Comparer { get; } = new ImmutableNameSetEqualityComparer();

        [NotNull]
        public static IImmutableNameSet Create(IEnumerable<string> values) => Create(values.ToArray());

        [NotNull]
        public static IImmutableNameSet Create([NotNull] params string[] names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            //if (names.Length == 0) throw new ArgumentException("You need to specify at least one name.", nameof(names));

            return new ImmutableNameSet(names);
        }

        private string DebuggerDisplay => ToString();

        //[NotNull]
        //public static IImmutableNameSet From([NotNull] Type type) => CommandNameFactory.From(type);        

        //[NotNull]
        //public static IImmutableNameSet From([NotNull] ICommand command) => From(command?.GetType());

        //[NotNull]
        //public static IImmutableNameSet From([NotNull] PropertyInfo parameterProperty) => immna ParameterNameFactory.From(parameterProperty);        

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

        public override bool Equals(object obj) => Comparer.Equals(this, obj as IImmutableSet<string>);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        public bool Equals(IImmutableSet<string> other) => Comparer.Equals(this, other);

        public bool Equals(IImmutableNameSet other) => Comparer.Equals(this, other);

        public override string ToString() => $"[{string.Join(", ", this)}]";

        public static bool operator ==(ImmutableNameSet left, ImmutableNameSet right) => Comparer.Equals(left, right);

        public static bool operator !=(ImmutableNameSet left, ImmutableNameSet right) => !(left == right);

        private sealed class ImmutableNameSetEqualityComparer : IEqualityComparer<IImmutableSet<string>>
        {
            public bool Equals(IImmutableSet<string> x, IImmutableSet<string> y)
            {
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return ReferenceEquals(x, y) || x.Overlaps(y) || (!x.Any() && !y.Any());
            }

            // The hash code are always different thus this comparer. We need to check if the sets overlap so we cannot relay on the hash code.
            public int GetHashCode(IImmutableSet<string> obj) => 0;
        }
    }
}
