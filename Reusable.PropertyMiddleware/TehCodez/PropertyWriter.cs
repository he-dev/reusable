using System;
using System.Collections.Generic;

namespace Reusable.ExpressProperty
{
    public class PropertyWriter<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public PropertyWriter<T> SetValue<TValue>(T obj, string propertyName, TValue value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue>(propertyName).Compile()
            )(obj, value);
            return this;
        }

        public PropertyWriter<T> SetValue<TValue, TIndex1>(T obj, string propertyName, TValue value, TIndex1 index1)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            propertyName = typeof(TIndex1).FullName;

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue, TIndex1>(propertyName).Compile()
            )(obj, value, index1);
            return this;
        }

        public PropertyWriter<T> SetValue<TValue, TIndex1, TIndex2>(T obj, string propertyName, TValue value, TIndex1 index1, TIndex2 index2)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            propertyName = typeof(TIndex1).FullName + ", " + typeof(TIndex2).FullName;

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue, TIndex1, TIndex2>(propertyName).Compile()
            )(obj, value, index1, index2);
            return this;
        }

        private TAction GetOrCreateSetterDelegate<TAction>(string propertyName, Func<TAction> setterDelegateFactory)
        {
            if (_cache.TryGetValue(propertyName, out object cacheItem))
            {
                return (TAction)cacheItem;
            }

            var getterDelegate = setterDelegateFactory();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (TAction)cacheItem;
        }
    }
}