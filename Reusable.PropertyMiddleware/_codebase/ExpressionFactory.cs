using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.PropertyMiddleware
{
    internal static class ExpressionFactory
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

    internal static class TypeExtensions
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}