using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Data
{
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

    [PublicAPI]
    public static class From<T> where T : INamespace
    {
        public static Key<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector, IKeyFactory keyFactory)
        {
            return new Key<TMember>(keyFactory.CreateKey(selector));
        }

        public static Key<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return Select(selector, KeyFactory.Default);
        }
    }

    [PublicAPI]
    public readonly struct Key<T>
    {
        public Key([NotNull] string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        [NotNull]
        public string Name { get; }

        public override string ToString() => Name;

        [NotNull]
        public static implicit operator string(Key<T> key) => key.Name;

        [NotNull]
        public static implicit operator SoftString(Key<T> key) => key.Name;
    }

    public static class KeyExtensions
    {
        public static Key<T> AppendIndex<T>(this Key<T> key, string item)
        {
            return new Key<T>($"{key}[{item}]");
        }
    }
}