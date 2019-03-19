using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using linq = System.Linq.Expressions;

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

        IExpressionContext Remove(SoftString key);
    }


//    public class ExpressionMetadata
//    {
//        public string DebugView => ExpressionContextScope.Current.ToDebugView();
//    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PublicAPI]
    public readonly struct ExpressionContext : IExpressionContext
    {
        [NotNull] private readonly IImmutableDictionary<SoftString, object> _data;

        public ExpressionContext([NotNull] IImmutableDictionary<SoftString, object> data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public static IExpressionContext Empty => new ExpressionContext(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Count);
            builder.DisplayCollection(x => x.Keys);
        });

        public object this[SoftString key] => _data[key];

        public int Count => _data.Count;

        public IEnumerable<SoftString> Keys => _data.Keys;

        public IEnumerable<object> Values => _data.Values;

        public bool ContainsKey(SoftString key) => _data.ContainsKey(key);

        public bool Contains(KeyValuePair<SoftString, object> pair) => _data.Contains(pair);

        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => _data.TryGetKey(equalKey, out actualKey);

        public bool TryGetValue(SoftString key, out object value) => _data.TryGetValue(key, out value);
        
        public IExpressionContext Remove(SoftString key) => new ExpressionContext(_data.Remove(key));

        public ExpressionContext Add(SoftString key, object value) => new ExpressionContext(_data.Add(key, value));

        public ExpressionContext TryAdd(SoftString key, object value) => _data.ContainsKey(key) ? this : new ExpressionContext(_data.Add(key, value));

        public IExpressionContext SetItem(SoftString key, object value) => new ExpressionContext(_data.SetItem(key, value));

        #region Helpers

        public static string CreateKey<T, TProperty>([NotNull] Item<T> item, [NotNull] linq.Expression<Func<T, TProperty>> propertySelector)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
            
            if (!(propertySelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException($"'{nameof(propertySelector)}' must be member-expression.");
            }

            var prefix = Regex.Replace(typeof(T).Name, "Context$", string.Empty);

            if (typeof(T).IsInterface)
            {
                prefix = Regex.Replace(prefix, "^I", string.Empty);
            }

            return $"{prefix}.{memberExpression.Member.Name}";
        }

        #endregion
    }
}