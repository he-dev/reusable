using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    public interface IImmutableSession : IEnumerable<(SoftString Key, object Value)>
    {
        object this[SoftString key] { get; }

        int Count { get; }

        IEnumerable<SoftString> Keys { get; }

        IEnumerable<object> Values { get; }

        bool ContainsKey(SoftString key);

        IImmutableSession SetItem(SoftString key, object value);

        bool TryGetValue(SoftString key, out object value);
    }

    // With a 'struct' we don't need any null-checks.
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    [PublicAPI]
    public readonly struct ImmutableSession : IImmutableSession
    {
        [CanBeNull] private readonly IImmutableDictionary<SoftString, object> _data;

        public ImmutableSession([NotNull] IImmutableDictionary<SoftString, object> metadata) => _data = metadata ?? throw new ArgumentNullException(nameof(metadata));

        public static ImmutableSession Empty => new ImmutableSession(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Count);
            builder.DisplayValues(x => x.Select(y => y.Key));
        });

        // A struct cannot be initialized so the field remains null when using 'default'.
        private IImmutableDictionary<SoftString, object> Data => _data ?? ImmutableDictionary<SoftString, object>.Empty;

        public object this[SoftString key] => Data[key];

        public int Count => Data.Count;

        public IEnumerable<SoftString> Keys => Data.Keys;

        public IEnumerable<object> Values => Data.Values;

        public bool ContainsKey(SoftString key) => Data.ContainsKey(key);      

        public bool TryGetValue(SoftString key, out object value) => Data.TryGetValue(key, out value);

        [MustUseReturnValue]
        public IImmutableSession Add(SoftString key, object value) => new ImmutableSession(Data.Add(key, value));

        //[MustUseReturnValue]
        //public Metadata TryAdd(SoftString key, object value) => Data.ContainsKey(key) ? this : new Metadata(Data.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        [MustUseReturnValue]
        public IImmutableSession SetItem(SoftString key, object value) => new ImmutableSession(Data.SetItem(key, value));
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

        public IEnumerator<(SoftString Key, object Value)> GetEnumerator() => Data.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
    }

    /// <summary>
    /// Provides a level of abstraction for metadata by adding the scope by T so that extensions can be grouped.
    /// </summary>
    [PublicAPI]
    public readonly struct ImmutableSession<TScope> where TScope : ISessionScope
    {
        public ImmutableSession(IImmutableSession metadata) => Value = metadata;

        public IImmutableSession Value { get; }

        public static string Key => Regex.Replace(typeof(TScope).ToPrettyString(), "^I", string.Empty);

//        [MustUseReturnValue]
//        public TValue Get<TValue>(Expression<Func<TScope, TValue>> getItem, TValue defaultValue = default)
//        {
//            return Value.TryGetValue(GetMemberName(getItem), out TValue value) ? value : defaultValue;
//        }
//
//        [MustUseReturnValue]
//        public Metadata<TScope> Set<TValue>(Expression<Func<TScope, TValue>> setItem, TValue value)
//        {
//            return Value.SetItem(GetMemberName(setItem), value);
//        }

        internal static string GetMemberName(LambdaExpression xItem)
        {
            return
                xItem.Body is MemberExpression me
                    ? me.Member.Name
                    : throw DynamicException.Create
                    (
                        $"NotMemberExpression",
                        $"Cannot use expression '{xItem}' because Get/Set expression must be member-expressions."
                    );
        }

//        public bool AssignValueWhenExists<TValue, TInstance>(Expression<Func<TScope, TValue>> getter, TInstance obj)
//        {
//            // obj.Property;
//            var setter = Expression.Property(Expression.Constant(obj), ((MemberExpression)getter.Body).Member.Name);
//            return AssignValueWhenExists(getter, Expression.Lambda<Func<TValue>>(setter));
//        }
//
//        public bool AssignValueWhenExists<TValue>(Expression<Func<TScope, TValue>> getter, Expression<Func<TValue>> setter)
//        {
//            var memberName = GetMemberName(getter);
//            if (Value.TryGetValue(memberName, out TValue value))
//            {
//                // obj.Property = value;
//                var assign = Expression.Assign(setter.Body, Expression.Constant(value));
//                ((Func<string>)Expression.Lambda(assign).Compile())();
//                return true;
//            }
//
//            return false;
//        }

        //public static implicit operator ImmutableSession<TScope>(ImmutableSession metadata) => new ImmutableSession<TScope>(metadata);

        //public static implicit operator ImmutableSession(ImmutableSession<TScope> scope) => scope.Value;
    }

    [PublicAPI]
    public readonly struct ImmutableSessionGetter<TScope> where TScope : ISessionScope
    {
        public ImmutableSessionGetter(IImmutableSession metadata) => Value = metadata;

        public IImmutableSession Value { get; }

        public static string Key => typeof(ImmutableSession<TScope>).ToPrettyString();

        [MustUseReturnValue]
        public TValue Get<TValue>(Expression<Func<TScope, TValue>> getItem, TValue defaultValue = default)
        {
            return Value.TryGetValue(ImmutableSession<TScope>.GetMemberName(getItem), out TValue value) ? value : defaultValue;
        }

        //public static implicit operator ImmutableSessionGetter<TScope>(ImmutableSession metadata) => new ImmutableSessionGetter<TScope>(metadata);

        //public static implicit operator ImmutableSession(ImmutableSessionGetter<TScope> scope) => scope.Value;
    }

    [PublicAPI]
    public readonly struct ImmutableSessionSetter<TScope> where TScope : ISessionScope
    {
        public ImmutableSessionSetter(IImmutableSession metadata) => Value = metadata;

        public IImmutableSession Value { get; }

        public static string Key => typeof(ImmutableSession<TScope>).ToPrettyString();

        [MustUseReturnValue]
        public ImmutableSessionSetter<TScope> Set<TValue>(Expression<Func<TScope, TValue>> setItem, TValue value)
        {
            return new ImmutableSessionSetter<TScope>(Value.SetItem(ImmutableSession<TScope>.GetMemberName(setItem), value));
        }

        //public static implicit operator ImmutableSessionSetter<TScope>(ImmutableSession metadata) => new ImmutableSessionSetter<TScope>(metadata);

        //public static implicit operator ImmutableSession(ImmutableSessionSetter<TScope> scope) => scope.Value;
    }

    public interface ISessionScope { }
}