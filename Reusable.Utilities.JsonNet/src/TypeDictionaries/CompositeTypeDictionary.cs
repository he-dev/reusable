using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.TypeDictionaries
{
    public class CompositeTypeDictionary : ITypeDictionary
    {
        private readonly IImmutableDictionary<string, Type> _types;

        public CompositeTypeDictionary(params ITypeDictionary[] typeDictionaries)
        {
            var items =
                from d in typeDictionaries
                from x in d
                group x by x.Key into g
                select g.First();

            _types = items.ToImmutableDictionary(x => x.Key, x => x.Value, SoftString.Comparer);
        }

        public static CompositeTypeDictionary Empty => new CompositeTypeDictionary();

        public Type? Resolve(string typeName)
        {
            return _types.TryGetValue(typeName, out var type) ? type : default;
        }

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _types.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_types).GetEnumerator();

        public static CompositeTypeDictionary operator +(CompositeTypeDictionary left, ITypeDictionary right) => new CompositeTypeDictionary(left, right);
    }
}