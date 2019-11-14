using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;


namespace Reusable.Commander
{
    public class NameSet : ImmutableKeySet<Name>
    {
        public NameSet([NotNull] IEnumerable<Name> names) : base(names) { }

        public NameSet([NotNull] params Name[] names)
            : this((IEnumerable<Name>)names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
        }

        public NameSet(params string[] names)
            : this(new NameSet(names.Skip(1).Select(n => new Name(n, Name.Options.Tag)).Prepend(new Name(names.First(), Name.Options.Default)))) { }

        public static NameSet Command { get; } = FromPosition(0);

        [NotNull]
        public Name Default => this.Single(n => n.Option.Contains(Name.Options.Default | Name.Options.CommandLine));

        public IEnumerable<Name> Tags => this.Where(n => n.Option.Contains(Name.Options.Tag));

        public static NameSet FromPosition(int position)
        {
            return new NameSet(new Name($"#{position}", Name.Options.CommandLine | Name.Options.Positional));
        }

        public static NameSet FromName(string name)
        {
            return new NameSet(new Name(name, Name.Options.CommandLine));
        }

        public override string ToString() => string.Join(", ", this.Select(x => x.ToString()));

        public static implicit operator NameSet(string name) => new NameSet((name, Name.Options.CommandLine));
    }

    public class Name : IEquatable<Name>
    {
        public Name(SoftString value, Option<Name> option)
        {
            Value = value;
            Option = option;
        }

        public Name(SoftString value) : this(value, Name.Options.CommandLine) { }

        [AutoEqualityProperty]
        public SoftString Value { get; }

        public Option<Name> Option { get; }

        public override string ToString() => Value.ToString();

        public bool Equals(Name other) => AutoEquality<Name>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is Name name && Equals(name);

        public override int GetHashCode() => AutoEquality<Name>.Comparer.GetHashCode(this);

        public static implicit operator Name((SoftString Name, Option<Name> Option) t) => new Name(t.Name, t.Option);

        public static class Options
        {
            public static readonly Option<Name> Default = Option<Name>.CreateWithCallerName();

            public static readonly Option<Name> Tag = Option<Name>.CreateWithCallerName();

            public static readonly Option<Name> CommandLine = Option<Name>.CreateWithCallerName();

            public static readonly Option<Name> Positional = Option<Name>.CreateWithCallerName();
        }
    }
}