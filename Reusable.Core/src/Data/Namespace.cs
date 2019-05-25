using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Data
{
    // ReSharper disable once UnusedTypeParameter - 'T'  is required.
    public interface INamespace<out T> where T : INamespace { }

//    [Obsolete]
//    public static class Use<T> where T : INamespace
//    {
//        [DebuggerNonUserCode]
//        public static INamespace<T> Namespace => default;
//    }

    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }


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

    public readonly struct Key<T>
    {
        public Key(string name) => Name = name;

        [NotNull]
        public string Name { get; }

        public static implicit operator string(Key<T> key) => key.Name;

        public static implicit operator SoftString(Key<T> key) => key.Name;
    }
}