using System;
using System.Collections.Generic;

namespace Reusable.PropertyMiddleware
{
    public class PropertyReader<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public TResult GetValue<TResult>(T obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TResult>(propertyName).Compile()
            )(obj);
        }

        public TResult GetValue<TIndex1, TResult>(T obj, string propertyName, TIndex1 index1)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            propertyName = typeof(TIndex1).FullName;
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TIndex1, TResult>(propertyName).Compile()
            )(obj, index1);
        }

        public TResult GetValue<TIndex1, TIndex2, TResult>(T obj, string propertyName, TIndex1 index1, TIndex2 index2)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            propertyName = typeof(TIndex1).FullName + ", " + typeof(TIndex2).FullName;
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TIndex1, TIndex2, TResult>(propertyName).Compile()
            )(obj, index1, index2);
        }

        private TFunc GetOrCreateGetterDelegate<TFunc>(string propertyName, Func<TFunc> getterDelegateFactory)
        {
            if (_cache.TryGetValue(propertyName, out object cacheItem))
            {
                return (TFunc)cacheItem;
            }

            var getterDelegate = getterDelegateFactory();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (TFunc)cacheItem;
        }
    }
}