using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.ExpressProperty
{
    internal class PropertyReader
    {
        private readonly Dictionary<string, Delegate> _cache = new Dictionary<string, Delegate>();

        private readonly Type _type;

        public PropertyReader(Type type)
        {
            _type = type;
        }

        //public object this[object obj, string propertyName, params object[] arguments]
        //{
        //    get
        //    {
        //        return GetOrCreateGetterDelegate(
        //            propertyName,
        //            () => GetterExpresssionFactory.CreateGetterExpression(obj, propertyName).Compile()
        //        )(obj);
        //    }
        //}

        public static Func<object, object, object> Create(Type type, bool nonPublic = false)
        {
            var properties = type.GetProperties(GetBindingFlags(nonPublic));
            foreach (var property in properties)
            {
                var indexParameters = property.GetIndexParameters();
                if (indexParameters.Any())
                {
                    var expr = GetterExpresssionFactory.CreateIndexExpression<Func<object, object, object>>(type, property, indexParameters).Compile();
                    return expr;
                }
                else
                {
                    var expr = GetterExpresssionFactory.CreateMemberExpression(type, property.Name).Compile();
                }
            }
            return null;
        }

        //private TFunc GetOrCreateGetterDelegate<TFunc>(string propertyName, Func<TFunc> getterDelegateFactory)
        //{
        //    if (_cache.TryGetValue(propertyName, out object cacheItem))
        //    {
        //        return (TFunc)cacheItem;
        //    }

        //    cacheItem = getterDelegateFactory();
        //    _cache.Add(propertyName, cacheItem);

        //    return (TFunc)cacheItem;
        //}

        private static BindingFlags GetBindingFlags(bool nonPublic)
        {
            var nonPublicFlag = nonPublic ? BindingFlags.NonPublic : BindingFlags.Default;
            return BindingFlags.Instance | nonPublicFlag | BindingFlags.Public;
        }
    }

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