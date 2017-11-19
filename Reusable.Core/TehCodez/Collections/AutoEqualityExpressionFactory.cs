using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Collections
{
    internal static class AutoEqualityExpressionFactory
    {
        private static readonly IDictionary<StringComparison, IEqualityComparer<string>> StringComparers = new Dictionary<StringComparison, IEqualityComparer<string>>
        {
            [StringComparison.CurrentCulture] = StringComparer.CurrentCulture,
            [StringComparison.CurrentCultureIgnoreCase] = StringComparer.CurrentCultureIgnoreCase,
            [StringComparison.InvariantCulture] = StringComparer.InvariantCulture,
            [StringComparison.InvariantCultureIgnoreCase] = StringComparer.InvariantCultureIgnoreCase,
            [StringComparison.Ordinal] = StringComparer.Ordinal,
            [StringComparison.OrdinalIgnoreCase] = StringComparer.OrdinalIgnoreCase,
        };

        public static Expression CreateEqualsExpression<TProperty>(
            PropertyInfo property,
            AutoEqualityPropertyAttribute attribute,
            Expression leftParameter,
            Expression rightParameter)
        {
            if (property.PropertyType == typeof(string) || property.PropertyType.IsEnum)
            {
                // Short-cut to have compiler write this expression for us.
                var equalsFunc = (Expression<Func<TProperty, TProperty, bool>>)((x, y) => GetComparer<TProperty>(attribute).Equals(x, y));

                return Expression.Invoke(
                    equalsFunc,
                    Expression.Property(leftParameter, property),
                    Expression.Property(rightParameter, property)
                );
            }

            var equalsMethod = GetEqualsMethod(property.PropertyType);

            return Expression.Call(Expression.Property(leftParameter, property), equalsMethod, Expression.Property(rightParameter, property));
        }

        [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")] // Using Type[] for consistency
        private static MethodInfo GetEqualsMethod(Type propertyType)
        {
            // Types that implement the IEquatable<T> interface should have a strong Equals method.
            
            var genericEquatable = typeof(IEquatable<>);
            var propertyEquatable = genericEquatable.MakeGenericType(propertyType);

            if (propertyType.GetInterfaces().Contains(propertyEquatable))
            {
                return propertyType.GetMethod(
                    nameof(IEquatable<object>.Equals),
                    new Type[] { propertyType }
                );
            }
            
            // In other cases just get the ordinary Equals.
            
            return propertyType.GetMethod(
                nameof(object.Equals),
                new Type[] { typeof(object) }
            );
        }

        // The 'rightParameter' argument is not used but by having the same signature for both methods allows for a few optimizations in other places.
        public static Expression CreateGetHashCodeExpression<TProperty>(
            PropertyInfo property,
            AutoEqualityPropertyAttribute attribute,
            Expression leftParameter,
            Expression rightParameter)
        {
            if (property.PropertyType == typeof(string) || property.PropertyType.IsEnum)
            {
                // Short-cut to have compiler write this expression for us.
                var getHashCodeFunc = (Expression<Func<TProperty, int>>)((obj) => GetComparer<TProperty>(attribute).GetHashCode(obj));

                return Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Property(leftParameter, property)
                );
            }

            // Call the instance 'GetHashCode' method by default.

            var getHashCodeMethod = property.PropertyType.GetMethod(nameof(GetHashCode));

            // ReSharper disable once AssignNullToNotNullAttribute - getHashCodeMethod is never null
            return Expression.Call(
                Expression.Property(leftParameter, property),
                getHashCodeMethod
            );
        }

        private static IEqualityComparer<TProperty> GetComparer<TProperty>(AutoEqualityPropertyAttribute attribute)
        {
            if (typeof(TProperty) == typeof(string))
            {
                return (IEqualityComparer<TProperty>)StringComparers[attribute.StringComparison];
            }

            return EqualityComparer<TProperty>.Default;
        }
    }
}