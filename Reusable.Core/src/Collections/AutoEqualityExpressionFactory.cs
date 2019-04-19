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
            var areEqualExpression = CreateIfThenExpression<TProperty, bool>(context, labelTarget, (left, right) => ReferenceEquals(left, right), true);
            var leftIsNullExpression = CreateIfThenExpression<TProperty, bool>(context, labelTarget, (left, right) => ReferenceEquals(left, null), false);
            var rightIsNullExpression = CreateIfThenExpression<TProperty, bool>(context, labelTarget, (left, right) => ReferenceEquals(left, null), false);

            return Expression.Block(
                areEqualExpression,
                leftIsNullExpression,
                rightIsNullExpression
            );
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
                //var getHashCodeFunc = (Expression<Func<TProperty, int>>)((obj) => ReferenceEquals(obj, null) ? 0 : GetComparer<TProperty>(context.Attribute).GetHashCode(obj));
                var getHashCodeFunc = (Expression<Func<TProperty, int>>)((obj) => GetComparer<TProperty>(context.Attribute).GetHashCode(obj));

                return Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Property(context.LeftParameter, context.Property)
                );
            }
            else
            {

                // Call the instance 'GetHashCode' method by default.

                //var labelTarget = Expression.Label(typeof(int));

                //var getHashCodeNullGuardExpression = CreateGetHashCodeNullGuardExpression<TProperty>(context, labelTarget);
                //var getHashCodeMethod = context.Property.PropertyType.GetMethod(nameof(GetHashCode));

                var getHashCodeFunc = (Expression<Func<TProperty, int>>) ((obj) => ReferenceEquals(obj, null) ? 0 : obj.GetHashCode());

                return Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Property(context.LeftParameter, context.Property)
                );
            }

            // ReSharper disable once AssignNullToNotNullAttribute - getHashCodeMethod is never null
            //var getHashCodeExpression = Expression.Call(
            //    Expression.Property(context.LeftParameter, context.Property),
            //    getHashCodeMethod
            //);

            //return Expression.Block(new[]
            //{
            //    getHashCodeNullGuardExpression,
            //    Expression.Return(labelTarget, getHashCodeExpression),
            //    Expression.Label(labelTarget, defaultValue: Expression.Constant(0))
            //});
        }

//        private static Expression CreateGetHashCodeNullGuardExpression<TProperty>(AutoEqualityPropertyContext context, LabelTarget labelTarget)
//        {
//            var propertyIsNullExpression = CreateIfThenExpression<TProperty, int>(labelTarget, (left, right) => ReferenceEquals(left, right), context.LeftParameter, Expression.Constant(null, typeof(TProperty)), 0);
//
//            return Expression.Block(propertyIsNullExpression);
//        }

        #endregion

        private static Expression CreateIfThenExpression<TProperty, TResult>(AutoEqualityPropertyContext context, LabelTarget labelTarget, Func<TProperty, TProperty, bool> referenceEquals, TResult result)
        {
            // Let the compiler create this expression for us.
            var referenceEqualsExpression = (Expression<Func<TProperty, TProperty, bool>>)((left, right) => referenceEquals(left, right));

            var referenceEqualsInvokeExpression = Expression.Invoke(
                referenceEqualsExpression,
                Expression.Property(context.LeftParameter, context.Property),
                Expression.Property(context.RightParameter, context.Property)
            );

            return Expression.IfThen(
                referenceEqualsInvokeExpression,
                Expression.Return(labelTarget, Expression.Constant(result))
            );
        }

//        private static Expression CreateIfThenExpression<TProperty, TResult>(LabelTarget labelTarget, Func<TProperty, TProperty, bool> referenceEquals, Expression objA, Expression objB, TResult result)
//        {
//            // Let the compiler create this expression for us.
//            var referenceEqualsExpression = (Expression<Func<TProperty, TProperty, bool>>)((left, right) => referenceEquals(left, right));
//
//            var referenceEqualsInvokeExpression = Expression.Invoke(
//                referenceEqualsExpression,
//                objA,
//                objB
//            );
//
//            return Expression.IfThen(
//                referenceEqualsInvokeExpression,
//                Expression.Return(labelTarget, Expression.Constant(result))
//            );
//        }

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