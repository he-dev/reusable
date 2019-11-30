using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;


namespace Reusable.Commander
{
    public class MultiName : IEnumerable<string>, IEquatable<MultiName>
    {
        private readonly List<string> _names;

        public MultiName(IEnumerable<string> names) => _names = new List<string>(names.Distinct(SoftString.Comparer));

        public MultiName(params string[] names) : this(names.AsEnumerable()) { }

        public static MultiName Empty => new MultiName();

        public static MultiName Command => new MultiName("Arg0");

        public static MultiName Create(params string[] names) => new MultiName(names);

        #region IEquatable<MultiName>

        public bool Equals(MultiName? other) => _names.Intersect(other?._names ?? Enumerable.Empty<string>(), SoftString.Comparer).Any();

        public override bool Equals(object? obj) => Equals(obj as MultiName);

        public override int GetHashCode() => 0;

        #endregion

        public override string ToString() => _names.Join(", ").EncloseWith("[]");

        #region IEnumerable<string>

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_names).GetEnumerator();

        #endregion
        
        public static implicit operator MultiName(string name) => new MultiName(name);
    }
}