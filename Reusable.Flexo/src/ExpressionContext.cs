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

        public ExpressionContext Add(SoftString key, object value) => new ExpressionContext(_data.Add(key, value));

        public ExpressionContext TryAdd(SoftString key, object value) => _data.ContainsKey(key) ? this : new ExpressionContext(_data.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        public IExpressionContext SetItem(SoftString key, object value) => new ExpressionContext(_data.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }
}