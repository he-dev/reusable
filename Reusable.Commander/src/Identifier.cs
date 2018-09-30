using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Commander
{
    public class Identifier : IEnumerable<SoftString>, IEquatable<Identifier>
    {
        private readonly IEnumerable<SoftString> _names;

        private readonly ImmutableKeySet<SoftString> _identifier;

        public Identifier([NotNull] IEnumerable<SoftString> names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            _names = names.ToList();
            _identifier = ImmutableKeySet.Create(_names);
        }

        public Identifier([NotNull] params SoftString[] names) : this((IEnumerable<SoftString>) names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
        }
        
        public static Identifier Empty { get; } = new Identifier(string.Empty);

        public SoftString Default => _names.First();

        public IEnumerable<SoftString> Aliases => _names.Skip(1);
        
        public static Identifier Create(params SoftString[] names) => new Identifier(names);

        #region IEnumerable<SoftString>

        public IEnumerator<SoftString> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEquatable<Identifier>

        public bool Equals(Identifier other) => _identifier.Equals(other?._identifier);

        public override bool Equals(object obj) => obj is Identifier other && Equals(other);

        // It's not possible to calculate the hashcode.
        public override int GetHashCode() => 0;

        #endregion
        
        public override string ToString() => string.Join(", ", this.Select(x => x.ToString()));

        #region operators

        public static implicit operator Identifier(string name) => new Identifier(name);

        public static implicit operator Identifier(SoftString name) => new Identifier(name);

        public static bool operator ==(Identifier x, Identifier y) => x?.Equals(y) == true;

        public static bool operator !=(Identifier x, Identifier y) => !(x == y);

        #endregion
    }
}