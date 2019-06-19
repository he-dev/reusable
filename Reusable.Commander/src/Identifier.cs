using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;

namespace Reusable.Commander
{
    public class Identifier : ImmutableKeySet<Name>
    {
        public Identifier([NotNull] IEnumerable<Name> names) : base(names) { }

        public Identifier([NotNull] params Name[] names)
            : this((IEnumerable<Name>)names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
        }

        public Identifier(params string[] names)
            : this(new Identifier(names.Skip(1).Select(n => new Name(n, NameOption.Alias)).Prepend(new Name(names.First(), NameOption.Default)))) { }

        public static Identifier Command { get; } = FromPosition(0);

        [NotNull]
        public Name Default => this.Single(n => n.Option.Contains(NameOption.Default | NameOption.CommandLine));

        public IEnumerable<Name> Aliases => this.Where(n => n.Option.Contains(NameOption.Alias));

        public static Identifier FromPosition(int position)
        {
            return new Identifier(new Name($"#{position}", NameOption.CommandLine | NameOption.Positional));
        }

        public static Identifier FromName(string name)
        {
            return new Identifier(new Name(name, NameOption.CommandLine));
        }

        public override string ToString() => string.Join(", ", this.Select(x => x.ToString()));
        
        public static implicit operator Identifier(string name) => new Identifier((name, NameOption.CommandLine));
    }

    public class Name : IEquatable<Name>
    {
        public Name(SoftString value, NameOption option)
        {
            Value = value;
            Option = option;
        }

        public Name(SoftString value) : this(value, NameOption.CommandLine) { }

        [AutoEqualityProperty]
        public SoftString Value { get; }

        public NameOption Option { get; }

        public override string ToString() => Value.ToString();

        public bool Equals(Name other) => AutoEquality<Name>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is Name name && Equals(name);

        public override int GetHashCode() => AutoEquality<Name>.Comparer.GetHashCode(this);

        public static implicit operator Name((SoftString Name, NameOption Option) t) => new Name(t.Name, t.Option);
        
        
    }

    public class NameOption : Option<NameOption>
    {
        public NameOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly NameOption Default = CreateWithCallerName();

        public static readonly NameOption Alias = CreateWithCallerName();

        public static readonly NameOption CommandLine = CreateWithCallerName();

        public static readonly NameOption Positional = CreateWithCallerName();
    }
}