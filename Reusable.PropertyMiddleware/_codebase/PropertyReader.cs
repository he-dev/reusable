using System;
using System.Collections.Generic;
using Reusable.Fuse;

namespace Reusable.PropertyMiddleware
{
    public class PropertyReader<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public TResult GetValue<TResult>(T obj, string propertyName)
        {
            obj.Validate(nameof(obj)).IsNotNull();
            propertyName.Validate(nameof(propertyName)).IsNotNullOrEmpty();

            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TResult>(propertyName).Compile()
            )(obj);
        }

        public TResult GetValue<TIndex1, TResult>(T obj, string propertyName, TIndex1 index1)
        {
            obj.Validate(nameof(obj)).IsNotNull();

            propertyName = typeof(TIndex1).FullName;
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TIndex1, TResult>(propertyName).Compile()
            )(obj, index1);
        }

        public TResult GetValue<TIndex1, TIndex2, TResult>(T obj, string propertyName, TIndex1 index1, TIndex2 index2)
        {
            obj.Validate(nameof(obj)).IsNotNull();

            propertyName = typeof(TIndex1).FullName + ", " + typeof(TIndex2).FullName;
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TIndex1, TIndex2, TResult>(propertyName).Compile()
            )(obj, index1, index2);
        }

        private TFunc GetOrCreateGetterDelegate<TFunc>(string propertyName, Func<TFunc> getterDelegateFactory)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
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