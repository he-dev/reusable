using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    // With a 'struct' we don't need any null-checks.
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    [PublicAPI]
    public readonly struct Metadata : IEnumerable<KeyValuePair<SoftString, object>>
    {
        [CanBeNull]
        private readonly IImmutableDictionary<SoftString, object> _data;

        public Metadata([NotNull] IImmutableDictionary<SoftString, object> metadata) => _data = metadata ?? throw new ArgumentNullException(nameof(metadata));

        public static Metadata Empty => new Metadata(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Count);
            builder.DisplayValues(x => x.Keys);
        });

        // A struct cannot be initialized so the field remains null when using 'default'.
        private IImmutableDictionary<SoftString, object> Data => _data ?? ImmutableDictionary<SoftString, object>.Empty;

        public object this[SoftString key] => Data[key];

        public int Count => Data.Count;

        public IEnumerable<SoftString> Keys => Data.Keys;

        public IEnumerable<object> Values => Data.Values;

        public bool ContainsKey(SoftString key) => Data.ContainsKey(key);

        public bool Contains(KeyValuePair<SoftString, object> pair) => Data.Contains(pair);

        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => Data.TryGetKey(equalKey, out actualKey);

        public bool TryGetValue(SoftString key, out object value) => Data.TryGetValue(key, out value);

        [MustUseReturnValue]
        public Metadata Add(SoftString key, object value) => new Metadata(Data.Add(key, value));

        //[MustUseReturnValue]
        //public Metadata TryAdd(SoftString key, object value) => Data.ContainsKey(key) ? this : new Metadata(Data.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        [MustUseReturnValue]
        public Metadata SetItem(SoftString key, object value) => new Metadata(Data.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();

        #region Scope

        // public Metadata<TScope> Scope<TScope>()
        // {
        //     var scopeKey = CreateScopeKey<TScope>();
        //     //return this.GetItemByCallerName(this, scopeKey);
        //     return this.TryGetValue(scopeKey, out Metadata value)
        //         ? new Metadata<TScope>(value)
        //         : new Metadata<TScope>(Empty);
        // }
        //
        // public Metadata Scope<TScope>(ConfigureMetadataScopeCallback<TScope> configureScope)
        // {
        //     // There might already be a cope defined so get the current one first. 
        //     var scope = configureScope(Scope<TScope>().Value);
        //     return this.SetItemByCallerName(scope.Value, CreateScopeKey<TScope>());
        // }

        // private static string CreateScopeKey<TScope>()
        // {
        //     return typeof(Metadata<TScope>).ToPrettyString();
        // }

        #endregion

        public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
    }

    /// <summary>
    /// Provides a level of abstraction for metadata by adding the scope by T so that extensions can be grouped.
    /// </summary>
    [PublicAPI]
    public readonly struct Metadata<TScope> // where T : IMetadataScope
    {
        public Metadata(Metadata metadata) => Value = metadata;

        public Metadata Value { get; }

        public static string Key => typeof(Metadata<TScope>).ToPrettyString();

        [MustUseReturnValue]
        public TValue Get<TValue>(Expression<Func<TScope, TValue>> getItem, TValue defaultValue = default)
        {
            return Value.TryGetValue(GetMemberName(getItem), out TValue value) ? value : defaultValue;
        }

        [MustUseReturnValue]
        public Metadata<TScope> Set<TValue>(Expression<Func<TScope, TValue>> setItem, TValue value)
        {
            return Value.SetItem(GetMemberName(setItem), value);
        }

        private string GetMemberName(LambdaExpression xItem)
        {
            return
                xItem.Body is MemberExpression me
                    ? me.Member.Name
                    : throw DynamicException.Create
                    (
                        "NotMemberExpression",
                        $"Cannot use expression '{xItem}' because Get/Set expression must be member-expressions."
                    );
        }

        public bool AssignValueWhenExists<TValue, TInstance>(Expression<Func<TScope, TValue>> setItem, TInstance obj)
        {
            var memberName = GetMemberName(setItem);
            if (Value.TryGetValue(memberName, out TValue value))
            {
                var property = typeof(TScope).GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (property is null)
                {
                    throw DynamicException.Create
                    (
                        $"PropertyNotFound",
                        $"Object of type '{typeof(TScope).ToPrettyString()}' does not have a property '{memberName}'."
                    );
                }
                property.SetValue(obj, value);
                return true;
            }
            return false;
        }
        
        //public bool AssignValueWhenExists<TValue, TInstance>(Expression<Func<TScope, TValue>> getItem, Expression<Func<TValue>> setItem)
        // {
        //     var memberName = GetMemberName(getItem);
        //     if (Value.TryGetValue(memberName, out TValue value))
        //     {
        //         if (setItem.Body is MemberExpression setMember)
        //         {
        //             ((PropertyInfo)setMember.Member);
        //         }
        //         var property = typeof(TScope).GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        //         if (property is null)
        //         {
        //             throw DynamicException.Create
        //             (
        //                 $"PropertyNotFound",
        //                 $"Object of type '{typeof(TScope).ToPrettyString()}' does not have a property '{memberName}'."
        //             );
        //         }
        //         //property.SetValue(obj, value);
        //         return true;
        //     }
        //     return false;
        // }

        public static implicit operator Metadata<TScope>(Metadata metadata) => new Metadata<TScope>(metadata);

        public static implicit operator Metadata(Metadata<TScope> scope) => scope.Value;
    }

    public interface IMetadataScope { }
}