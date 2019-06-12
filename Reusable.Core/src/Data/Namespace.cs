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
            return new Selector<TMember>(selector);
        }
    }

    public class Selector<T>
    {
        private readonly LambdaExpression _selector;

        public Selector(LambdaExpression selector)
        {
            _selector = selector;
            Keys = _selector.GetKeys().ToImmutableList();
        }

        private Selector(Selector<T> other, IImmutableList<Key> keys)
        {
            _selector = other._selector;
            Keys = keys;
        }

        [NotNull, ItemNotNull]
        public IImmutableList<Key> Keys { get; }

        public override string ToString() => _selector.ToString();

        public Selector<T> Index(string index)
        {
            if (Keys.OfType<IndexKey>().Any()) throw new InvalidOperationException("This selector already contains an index.");

            return new Selector<T>(this, Keys.Add(new IndexKey(index)));
        }

        [NotNull]
        public static implicit operator string(Selector<T> selector) => selector.Keys.Join(string.Empty);

        [NotNull]
        public static implicit operator SoftString(Selector<T> keys) => (string)keys;
    }
}