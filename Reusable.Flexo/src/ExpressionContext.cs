using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo
{
    public interface IExpressionContext
    {
        object this[SoftString key] { get; }

        int Count { get; }

        IEnumerable<SoftString> Keys { get; }

        IEnumerable<object> Values { get; }

        bool ContainsKey(SoftString key);

        IExpressionContext SetItem(SoftString key, object value);

        bool TryGetValue(SoftString key, out object value);
    }

    public class ExpressionMetadata
    {
        public string DebugView => ExpressionContextScope.Current.ToDebugView();
    }

    // With a 'struct' we don't need any null-checks.
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PublicAPI]
    public readonly struct ExpressionContext : IExpressionContext
    {
        [CanBeNull]
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        public ExpressionContext(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static IExpressionContext Empty => new ExpressionContext(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Count);
            builder.DisplayCollection(x => x.Keys);
        });

        private IImmutableDictionary<SoftString, object> Value => _metadata ?? ImmutableDictionary<SoftString, object>.Empty;

        public object this[SoftString key] => Value[key];

        public int Count => Value.Count;

        public IEnumerable<SoftString> Keys => Value.Keys;

        public IEnumerable<object> Values => Value.Values;

        public bool ContainsKey(SoftString key) => Value.ContainsKey(key);

        public bool Contains(KeyValuePair<SoftString, object> pair) => Value.Contains(pair);

        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => Value.TryGetKey(equalKey, out actualKey);

        public bool TryGetValue(SoftString key, out object value) => Value.TryGetValue(key, out value);

        public ExpressionContext Add(SoftString key, object value) => new ExpressionContext(Value.Add(key, value));

        public ExpressionContext TryAdd(SoftString key, object value) => Value.ContainsKey(key) ? this : new ExpressionContext(Value.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        public IExpressionContext SetItem(SoftString key, object value) => new ExpressionContext(Value.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }

    public static class ExpressionContext2Extensions
    {
        public static TProperty Get<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            Expression<Func<TExpression, TProperty>> propertySelector
        )
        {
            if (context.TryGetValue(CreateKey(item, propertySelector), out var value))
            {
                if (value is TProperty result)
                {
                    return result;
                }
                else
                {
                    throw new ArgumentException
                    (
                        $"There is a value for '{CreateKey(item, propertySelector)}' " +
                        $"but its type '{value.GetType().ToPrettyString()}' " +
                        $"is different from '{typeof(TProperty).ToPrettyString()}'"
                    );
                }
            }
            else
            {
                if (((MemberExpression)propertySelector.Body).Member.IsDefined(typeof(RequiredAttribute)))
                {
                    throw DynamicException.Create("RequiredValueMissing", $"{CreateKey(item, propertySelector)} is required.");
                }

                return default;
            }
        }

        public static IExpressionContext Set<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            Expression<Func<TExpression, TProperty>> selectProperty,
            TProperty value
        )
        {
            return context.SetItem(CreateKey(item, selectProperty), value);
        }

        private static string CreateKey<T, TProperty>
        (
            Item<T> item,
            Expression<Func<T, TProperty>> propertySelector
        )
        {
            if (!(propertySelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException($"'{nameof(propertySelector)}' must be member-expression.");
            }

            return $"{typeof(T).ToPrettyString()}.{memberExpression.Member.Name}";
        }
    }

    public class Item<T>
    { }

    public static class Item
    {
        public static Item<T> For<T>() => new Item<T>();
    }

    // public class ExpressionContextItemAttribute : Attribute
    // {
    //     public ExpressionContextItemAttribute(string scope)
    //     {
    //         Scope = scope;
    //     }
    //
    //     public ExpressionContextItemAttribute(Type scopeProvider) : this(scopeProvider.ToPrettyString())
    //     { }
    //
    //     public string Scope { get; }
    // }
}