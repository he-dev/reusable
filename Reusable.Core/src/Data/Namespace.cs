using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Data
{
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

    [PublicAPI]
    public static class From<T> where T : INamespace
    {
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return new Selector<TMember>(selector.GetKeys().ToImmutableList());
        }

//        public static Name<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
//        {
//            return Select(selector);
//        }
    }

    public class Selector<T>
    {
        public Selector(IImmutableList<Key> keys) => Keys = keys;

        [NotNull, ItemNotNull]
        public IImmutableList<Key> Keys { get; }

        public override string ToString() => this;

        [NotNull]
        public static implicit operator string(Selector<T> selector) => selector.Keys.Join(string.Empty);

        [NotNull]
        public static implicit operator SoftString(Selector<T> keys) => (string)keys;
    }
}