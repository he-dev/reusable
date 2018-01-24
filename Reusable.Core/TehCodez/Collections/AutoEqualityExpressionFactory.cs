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

        #region Equals method

        public static Expression CreateEqualsExpression<TProperty>(AutoEqualityPropertyContext context)
        {
            var labelTarget = Expression.Label(typeof(bool));

            var nullGuardBlockExpression = CreateNullGuardBlockExpression<TProperty>(context, labelTarget);
            var equalsMethodExpression = CreateEqualsMethodExpression<TProperty>(context);

            return Expression.Block(new[]
            {
                nullGuardBlockExpression,
                Expression.Return(labelTarget, equalsMethodExpression),
                Expression.Label(labelTarget, defaultValue: Expression.Constant(false))
            });
        }

        // Creates the three canonical null guard conditions.
        private static Expression CreateNullGuardBlockExpression<TProperty>(AutoEqualityPropertyContext context, LabelTarget labelTarget)
        {
            var areEqualExpression = CreateIfThenExpression((left, right) => ReferenceEquals(left, right), true);
            var leftIsNullExpression = CreateIfThenExpression((left, right) => ReferenceEquals(left, null), false);
            var rightIsNullExpression = CreateIfThenExpression((left, right) => ReferenceEquals(left, null), false);

            return Expression.Block(new[]
            {
                areEqualExpression,
                leftIsNullExpression,
                rightIsNullExpression,
            });

            Expression CreateIfThenExpression(Func<TProperty, TProperty, bool> referenceEquals, bool result)
            {
                var referenceEquasExpression = (Expression<Func<TProperty, TProperty, bool>>)((left, right) => referenceEquals(left, right));

                var referenceEquasInvokeExpression = Expression.Invoke(
                    referenceEquasExpression,
                    Expression.Property(context.LeftParameter, context.Property),
                    Expression.Property(context.RightParameter, context.Property)
                );

                return Expression.IfThen(
                    referenceEquasInvokeExpression,
                    Expression.Return(labelTarget, Expression.Constant(result))
                );
            }
        }

        private static Expression CreateEqualsMethodExpression<TProperty>(AutoEqualityPropertyContext context)
        {
            if (HasDefaultComparer(context.Property.PropertyType))
            {
                // Short-cut to have compiler write this expression for us.
                var equalsFunc = (Expression<Func<TProperty, TProperty, bool>>)((x, y) => GetComparer<TProperty>(context.Attribute).Equals(x, y));

                return Expression.Invoke(
                    equalsFunc,
                    Expression.Property(context.LeftParameter, context.Property),
                    Expression.Property(context.RightParameter, context.Property)
                );
            }

            var equalsMethod = GetEqualsMethod(context.Property.PropertyType);
            return Expression.Call(
                Expression.Property(context.LeftParameter, context.Property), 
                equalsMethod, 
                Expression.Property(context.RightParameter, context.Property)
            );
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

        #endregion

        #region GetHashCode method 

        // The 'rightParameter' argument is not used but by having the same signature for both methods allows for a few optimizations in other places.
        public static Expression CreateGetHashCodeExpression<TProperty>(AutoEqualityPropertyContext context)
        {
            if (HasDefaultComparer(context.Property.PropertyType))
            {
                // Short-cut to have compiler write this expression for us.
                var getHashCodeFunc = (Expression<Func<TProperty, int>>)((obj) => ReferenceEquals(obj, null) ? 0 : GetComparer<TProperty>(context.Attribute).GetHashCode(obj));

                return Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Property(context.LeftParameter, context.Property)
                );
            }

            // Call the instance 'GetHashCode' method by default.

            var getHashCodeMethod = context.Property.PropertyType.GetMethod(nameof(GetHashCode));

            // ReSharper disable once AssignNullToNotNullAttribute - getHashCodeMethod is never null
            return Expression.Call(
                Expression.Property(context.LeftParameter, context.Property),
                getHashCodeMethod
            );
        }

        #endregion

        private static bool HasDefaultComparer(Type type)
        {
            if (type == typeof(string)) return true;
            if (type.IsEnum) return true;

            return false;
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