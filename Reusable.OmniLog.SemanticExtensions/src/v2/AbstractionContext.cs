using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.OmniLog.SemanticExtensions.v2
{
    using Reusable.OmniLog.v2;
    using v1 = Reusable.OmniLog.Abstractions;
    using v2 = Reusable.OmniLog.Abstractions.v2;

    public interface IAbstractionContext : IAbstractionLayer, IAbstractionCategory
    {
        IImmutableDictionary<string, object> Values { get; }
    }
    
    [PublicAPI]
    public readonly struct AbstractionContext : IAbstractionContext
    {
        public IImmutableDictionary<string, object> Values { get; }

        public AbstractionContext(IImmutableDictionary<string, object> values)
        {
            Values = values;
        }

        public AbstractionContext(IImmutableDictionary<string, object> values, string property, [CallerMemberName] string name = null)
            : this(values.Add(property, name)) { }

        public AbstractionContext(string property, [CallerMemberName] string name = null)
            : this(ImmutableDictionary<string, object>.Empty.Add(property, name)) { }

        public static IAbstractionContext Empty => new AbstractionContext();

        public object GetItem(string name)
        {
            return Values[name];
        }

        public IAbstractionContext SetItem(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return new AbstractionContext((Values ?? ImmutableDictionary<string, object>.Empty).Add(name, value));
        }

        public static class PropertyNames
        {
            public const string Layer = nameof(Layer);
            public const string Level = nameof(Level);
            public const string Category = nameof(Category);
            public const string Identifier = nameof(Identifier);
            public const string Snapshot = nameof(Snapshot);
            public const string Routine = nameof(Routine);
            public const string Because = nameof(Because);
        }
    }

    public static class ObjectExtensions
    {
        public static IEnumerable<(string Name, object Value)> EnumerateProperties<T>(this T obj)
        {
            return
                obj is IDictionary<string, object> dictionary
                    ? dictionary.Select(item => (item.Key, item.Value))
                    : obj
                        .GetType()
                        //.ValidateIsAnonymous()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property => (property.Name, property.GetValue(obj)));
        }

        private static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type or a dictionary");
        }
    }
}