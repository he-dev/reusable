using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Experimental
{
    public class PropertyReader<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();        
        
        public TResult GetValue<TResult>(T obj, string propertyName)
        {
            return GetOrCreateGetterDelegate<TResult>(propertyName)(obj);
        }

        //public TResult GetValue<TIndex1, TResult>(T instance, string propertyName, TIndex1 index1)
        //{
        //    return GetterCache[propertyName].Invoke(instance);
        //}

        //public TResult GetValue<TIndex1, TIndex2, TResult>(T instance, string propertyName, TIndex1 index1, TIndex2 index2)
        //{
        //    return GetterCache[propertyName].Invoke(instance);
        //}        

        private Func<T, TResult> GetOrCreateGetterDelegate<TResult>(string propertyName)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
            {
                return (Func<T, TResult>)cacheItem;
            }

            var getterDelegate = ExpressionFactory.CreateGetterExpression<T, TResult>(propertyName).Compile();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (Func<T, TResult>)cacheItem;
        }        
    }

    public class PropertyWriter<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public PropertyWriter<T> SetValue<TValue>(T obj, string propertyName, TValue value)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyName == null) { throw new ArgumentNullException(paramName: nameof(propertyName)); }

            GetOrCreateSetterDelegate<TValue>(propertyName)(obj, value);
            return this;
        }        

        private Action<T, TValue> GetOrCreateSetterDelegate<TValue>(string propertyName)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
            {
                return (Action<T, TValue>)cacheItem;
            }

            var getterDelegate = ExpressionFactory.CreateSetterExpression<T, TValue>(propertyName).Compile();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (Action<T, TValue>)cacheItem;
        }
    }

   

    public static class TypeAccessorExtensions
    {
        public static TValue GetValue<T, TValue>(this TypeAccessor<T> accessor, T obj, Expression<Func<T, TValue>> property)
        {
            var propertyName = GetMemberName(property);
            return accessor.GetValue<TValue>(obj, propertyName);
        }

        public static Dictionary<string, object> GetValues<T>(this TypeAccessor<T> accessor, T obj, params string[] propertyNames)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyNames == null) { throw new ArgumentNullException(paramName: nameof(propertyNames)); }

            return propertyNames.ToDictionary(
                       propertyName => propertyName,
                       propertyName => accessor.GetValue<object>(obj, propertyName)
            );
        }

        public static Dictionary<string, object> GetValues<T>(this TypeAccessor<T> accessor, T obj, IEnumerable<Expression<Func<T, object>>> properties)
        {
            return properties.Select(GetMemberName).ToDictionary(
                       propertyName => propertyName,
                       propertyName => accessor.GetValue<object>(obj, propertyName));
        }

        public static TypeAccessor<T> SetValue<T, TValue>(this TypeAccessor<T> accessor, T obj, Expression<Func<T, TValue>> property, TValue value)
        {
            var propertyName = GetMemberName(property);
            return accessor.SetValue(obj, propertyName, value);
        }

        public static TypeAccessor<T> SetValues<T>(this TypeAccessor<T> accessor, T instance, IDictionary<string, object> properties)
        {
            foreach (var property in properties)
            {
                accessor.SetValue(instance, property.Key, property.Value);
            }
            return accessor;
        }

        private static string GetMemberName(Expression expression)
        {
            var lambdaExpr = (LambdaExpression)expression;
            var memberExpr = lambdaExpr.Body as MemberExpression;
            if (memberExpr == null) { throw new ArgumentException("Expression must be a member expression."); }
            return memberExpr.Member.Name;
        }
    }

    public static class TypeExtensions
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }
    }

    public static class ExpressionFactory
    {
        private static BindingFlags GetBindingFlags(bool nonPublic)
        {
            var nonPublicFlag = nonPublic ? BindingFlags.NonPublic : BindingFlags.Default;
            return BindingFlags.Instance | nonPublicFlag | BindingFlags.Public;
        }

        public static Expression<Func<T, TProperty>> CreateGetterExpression<T, TProperty>(
            this PropertyInfo propertyInfo,
            bool nonPublic = false)
        {
            var hasGetter = propertyInfo.GetGetMethod(nonPublic) != null;
            if (!hasGetter || propertyInfo.GetIndexParameters().Any())
            {
                return null;
            }

            var obj = Expression.Parameter(typeof(T), "obj");
            var property = Expression.Property(obj, propertyInfo);
            return Expression.Lambda<Func<T, TProperty>>(property, obj);
        }

        public static Expression<Func<T, TProperty>> CreateGetterExpression<T, TProperty>(
            string propertyName,
            bool nonPublic = false)
        {
            return
                typeof(T)
                .GetProperty(propertyName, GetBindingFlags(nonPublic))
                .CreateGetterExpression<T, TProperty>();
        }

        public static Expression<Func<T, TIndex1, TProperty>> CreateGetterExpression<T, TIndex1, TProperty>(
                this PropertyInfo propertyInfo,
                bool nonPublic = false)
        {
            var hasGetter = propertyInfo.GetGetMethod(nonPublic) != null;
            if (!hasGetter || propertyInfo.GetIndexParameters().Length != 1)
            {
                return null;
            }

            var obj = Expression.Parameter(typeof(T), "obj");
            var index1 = Expression.Parameter(typeof(TIndex1), "i");
            var property = Expression.Property(obj, propertyInfo, index1);
            return Expression.Lambda<Func<T, TIndex1, TProperty>>(property, obj, index1);
        }

        public static Expression<Func<T, TIndex1, TIndex2, TProperty>> CreateGetterExpression<T, TIndex1, TIndex2, TProperty>(
            this PropertyInfo propertyInfo,
            bool nonPublic = false)
        {
            var hasGetter = propertyInfo.GetGetMethod(nonPublic) != null;
            if (!hasGetter || propertyInfo.GetIndexParameters().Length != 2)
            {
                return null;
            }

            var obj = Expression.Parameter(typeof(T), "obj");
            var index1 = Expression.Parameter(typeof(TIndex1), "i");
            var index2 = Expression.Parameter(typeof(TIndex2), "j");
            var property = Expression.Property(obj, propertyInfo, index1, index2);
            return Expression.Lambda<Func<T, TIndex1, TIndex2, TProperty>>(property, obj, index1, index2);
        }

        public static Expression<Action<T, TValue>> CreateSetterExpression<T, TValue>(this PropertyInfo propertyInfo)
        {
            var obj = Expression.Parameter(typeof(T), "obj");
            var value = Expression.Parameter(typeof(TValue), "value");
            var property = Expression.Property(obj, propertyInfo);
            return Expression.Lambda<Action<T, TValue>>(Expression.Assign(property, value), obj, value);
        }

        public static Expression<Action<T, TValue>> CreateSetterExpression<T, TValue>(
            string propertyName,
            bool nonPublic = false)
        {
            return
                typeof(T)
                .GetProperty(propertyName, GetBindingFlags(nonPublic))
                .CreateSetterExpression<T, TValue>();
        }

        public static Expression<Func<T>> CreateDefaultConstructorExpression<T>()
        {
            return
                typeof(T).HasDefaultConstructor()
                ? Expression.Lambda<Func<T>>(Expression.New(typeof(T)))
                : null;
        }
    }

    public class TypeAccessor<T>
    {
        private readonly Func<T> _createType;

        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public TypeAccessor()
        {
            _createType = ExpressionFactory.CreateDefaultConstructorExpression<T>()?.Compile();
        }

        public static TypeAccessor<TSource> Create<TSource>()
        {
            return new TypeAccessor<TSource>();
        }

        public T New()
        {
            if (_createType == null) { throw new InvalidOperationException($"Type '{typeof(T).Name}' does not have a default constructor."); }
            return _createType();
        }

        public TResult GetValue<TResult>(T obj, string propertyName)
        {
            return GetOrCreateGetterDelegate<TResult>(propertyName)(obj);
        }

        //public TResult GetValue<TIndex1, TResult>(T instance, string propertyName, TIndex1 index1)
        //{
        //    return GetterCache[propertyName].Invoke(instance);
        //}

        //public TResult GetValue<TIndex1, TIndex2, TResult>(T instance, string propertyName, TIndex1 index1, TIndex2 index2)
        //{
        //    return GetterCache[propertyName].Invoke(instance);
        //}

        public TypeAccessor<T> SetValue<TValue>(T obj, string propertyName, TValue value)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyName == null) { throw new ArgumentNullException(paramName: nameof(propertyName)); }

            GetOrCreateSetterDelegate<TValue>(propertyName)(obj, value);
            return this;
        }

        private Func<T, TResult> GetOrCreateGetterDelegate<TResult>(string propertyName)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
            {
                return (Func<T, TResult>)cacheItem;
            }

            var getterDelegate = ExpressionFactory.CreateGetterExpression<T, TResult>(propertyName).Compile();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (Func<T, TResult>)cacheItem;
        }

        private Action<T, TValue> GetOrCreateSetterDelegate<TValue>(string propertyName)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
            {
                return (Action<T, TValue>)cacheItem;
            }

            var getterDelegate = ExpressionFactory.CreateSetterExpression<T, TValue>(propertyName).Compile();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (Action<T, TValue>)cacheItem;
        }
    }
}
