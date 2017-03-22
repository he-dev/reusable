using System;
using System.Collections;
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
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TResult>(propertyName).Compile()
            )(obj);
        }

        public TResult GetValue<TIndex1, TResult>(T obj, string propertyName, TIndex1 index1)
        {
            propertyName = typeof(TIndex1).FullName;
            return GetOrCreateGetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateGetterExpression<T, TIndex1, TResult>(propertyName).Compile()
            )(obj, index1);
        }

        public TResult GetValue<TIndex1, TIndex2, TResult>(T obj, string propertyName, TIndex1 index1, TIndex2 index2)
        {
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

    public class PropertyWriter<T>
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public PropertyWriter<T> SetValue<TValue>(T obj, string propertyName, TValue value)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyName == null) { throw new ArgumentNullException(paramName: nameof(propertyName)); }

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue>(propertyName).Compile()
            )(obj, value);
            return this;
        }

        public PropertyWriter<T> SetValue<TValue, TIndex1>(T obj, string propertyName, TValue value, TIndex1 index1)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }

            propertyName = typeof(TIndex1).FullName;

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue, TIndex1>(propertyName).Compile()
            )(obj, value, index1);
            return this;
        }

        public PropertyWriter<T> SetValue<TValue, TIndex1, TIndex2>(T obj, string propertyName, TValue value, TIndex1 index1, TIndex2 index2)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }

            propertyName = typeof(TIndex1).FullName + ", " + typeof(TIndex2).FullName;

            GetOrCreateSetterDelegate(
                propertyName,
                () => ExpressionFactory.CreateSetterExpression<T, TValue, TIndex1, TIndex2>(propertyName).Compile()
            )(obj, value, index1, index2);
            return this;
        }

        private TAction GetOrCreateSetterDelegate<TAction>(string propertyName, Func<TAction> setterDelegateFactory)
        {
            var cacheItem = default(object);
            if (_cache.TryGetValue(propertyName, out cacheItem))
            {
                return (TAction)cacheItem;
            }

            var getterDelegate = setterDelegateFactory();
            cacheItem = getterDelegate;
            _cache.Add(propertyName, cacheItem);

            return (TAction)cacheItem;
        }
    }

    public class ExpressionList<T> : List<Expression<Func<T, object>>> { }

    public class ExpressionDictionary<T> : Dictionary<Expression<Func<T, object>>, object> { }

    public static class PropertyReaderExtensions
    {
        public static TValue GetValue<T, TValue>(this PropertyReader<T> reader, T obj, Expression<Func<T, TValue>> property)
        {
            var propertyName = property.GetMemberName();
            return reader.GetValue<TValue>(obj, propertyName);
        }

        public static Dictionary<string, object> GetValues<T>(this PropertyReader<T> reader, T obj, params string[] propertyNames)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyNames == null) { throw new ArgumentNullException(paramName: nameof(propertyNames)); }

            return propertyNames.ToDictionary(
                propertyName => propertyName,
                propertyName => reader.GetValue<object>(obj, propertyName)
            );
        }

        public static Dictionary<string, object> GetValues<T>(this PropertyReader<T> reader, T obj, ExpressionList<T> properties)
        {
            return properties.Select(p => p.GetMemberName()).ToDictionary(
                propertyName => propertyName,
                propertyName => reader.GetValue<object>(obj, propertyName));
        }
    }

    public static class PropertyWriterExtensions
    {
        public static PropertyWriter<T> SetValue<T, TValue>(this PropertyWriter<T> writer, T obj, Expression<Func<T, TValue>> property, TValue value)
        {
            var propertyName = property.GetMemberName();
            return writer.SetValue(obj, propertyName, value);
        }

        public static PropertyWriter<T> SetValues<T>(this PropertyWriter<T> writer, T obj, ExpressionDictionary<T> properties)
        {
            foreach (var property in properties)
            {
                writer.SetValue(obj, property.Key, property.Value);
            }
            return writer;
        }
    }

    public static class TypeExtensions
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }
    }

    public static class ExpressionExtensions
    {
        public static string GetMemberName(this Expression expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda == null) { throw new ArgumentException("Expression must be a lambda expression."); }
            var memberExpression =
                (lambda.Body as MemberExpression) ??
                (lambda.Body as UnaryExpression)?.Operand as MemberExpression;

            if (memberExpression == null) { throw new ArgumentException("Expression must be a body expression."); }

            return memberExpression.Member.Name;
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
            bool nonPublic = false
        )
        {
            return
                typeof(T)
                .GetProperty(propertyName, GetBindingFlags(nonPublic))
                .CreateGetterExpression<T, TProperty>();
        }

        public static Expression<Func<T, TIndex1, TProperty>> CreateGetterExpression<T, TIndex1, TProperty>(
                string propertyName,
                bool nonPublic = false
        )
        {
            var propertyInfo = FindProperty<T, TIndex1>(nonPublic);

            var hasGetter = propertyInfo?.GetGetMethod(nonPublic) != null;
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
            string propertyName,
            bool nonPublic = false
        )
        {
            var propertyInfo = FindProperty<T, TIndex1, TIndex2>(nonPublic);

            var hasGetter = propertyInfo?.GetGetMethod(nonPublic) != null;
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

        public static Expression<Action<T, TValue>> CreateSetterExpression<T, TValue>(
            string propertyName,
            bool nonPublic = false
        )
        {
            var propertyInfo = typeof(T).GetProperty(propertyName, GetBindingFlags(nonPublic));

            var obj = Expression.Parameter(typeof(T), "obj");
            var value = Expression.Parameter(typeof(TValue), "value");
            var property = Expression.Property(obj, propertyInfo);
            return Expression.Lambda<Action<T, TValue>>(Expression.Assign(property, value), obj, value);
        }

        public static Expression<Action<T, TValue, TIndex1>> CreateSetterExpression<T, TValue, TIndex1>(
            string propertyName,
            bool nonPublic = false
        )
        {
            var propertyInfo = FindProperty<T, TIndex1>(nonPublic);

            var obj = Expression.Parameter(typeof(T), "obj");
            var index1 = Expression.Parameter(typeof(TIndex1), "i");
            var value = Expression.Parameter(typeof(TValue), "value");
            var property = Expression.Property(obj, propertyInfo, index1);
            return Expression.Lambda<Action<T, TValue, TIndex1>>(Expression.Assign(property, value), obj, value, index1);
        }


        public static Expression<Action<T, TValue, TIndex1, TIndex2>> CreateSetterExpression<T, TValue, TIndex1, TIndex2>(
            string propertyName,
            bool nonPublic = false
        )
        {
            var propertyInfo = FindProperty<T, TIndex1, TIndex2>(nonPublic);

            var obj = Expression.Parameter(typeof(T), "obj");
            var index1 = Expression.Parameter(typeof(TIndex1), "i");
            var index2 = Expression.Parameter(typeof(TIndex2), "j");
            var value = Expression.Parameter(typeof(TValue), "value");
            var property = Expression.Property(obj, propertyInfo, index1, index2);
            return Expression.Lambda<Action<T, TValue, TIndex1, TIndex2>>(Expression.Assign(property, value), obj, value, index1, index2);
        }

        private static PropertyInfo FindProperty<T, TIndex1>(bool nonPublic = false)
        {
            var propertyInfo =
                typeof(T)
                .GetProperties(GetBindingFlags(nonPublic))
                .SingleOrDefault(p =>
                    p.GetIndexParameters()
                    .Select(pi => pi.ParameterType)
                    .SequenceEqual(new[] { typeof(TIndex1) })
                );
            return propertyInfo;
        }

        private static PropertyInfo FindProperty<T, TIndex1, TIndex2>(bool nonPublic = false)
        {
            var propertyInfo =
                typeof(T)
                .GetProperties(GetBindingFlags(nonPublic))
                .SingleOrDefault(p =>
                    p.GetIndexParameters()
                    .Select(pi => pi.ParameterType)
                    .SequenceEqual(new[] { typeof(TIndex1), typeof(TIndex2) })
                );
            return propertyInfo;
        }

        public static Expression<Func<T>> CreateDefaultConstructorExpression<T>()
        {
            return
                typeof(T).HasDefaultConstructor()
                ? Expression.Lambda<Func<T>>(Expression.New(typeof(T)))
                : null;
        }
    }
}
