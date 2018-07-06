using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Linq.Expressions;

namespace Reusable.Validation
{
    public static class DuckValidatorExtensions2
    {
        #region IsValidWhen overloads

        public static DuckValidatorBuilder<T> IsValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DuckValidationRuleOptions options)
        {
            return builder.Add(new DuckValidationRule<T>(expression, messageFactory, options));
        }

        public static DuckValidatorBuilder<T> IsValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return builder.IsValidWhen(expression, messageFactory, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, string message)
        {
            return builder.IsValidWhen(expression, _ => message, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, DuckValidationRuleOptions options)
        {
            return builder.IsValidWhen(expression, _ => null, options);
        }

        public static DuckValidatorBuilder<T> IsValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.IsValidWhen(expression, _ => null, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsValidWhenNull<T>(this DuckValidatorBuilder<T> builder)
        {
            return builder.IsValidWhen(CreateIsNullLambdaExpression<T>(), _ => null, DuckValidationRuleOptions.BreakOnFailure);
        }

        #endregion

        #region IsNotValidWhen overloads

        public static DuckValidatorBuilder<T> IsNotValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DuckValidationRuleOptions options)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return builder.IsValidWhen(notExpression, messageFactory, options);
        }

        public static DuckValidatorBuilder<T> IsNotValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return builder.IsNotValidWhen(expression, messageFactory, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsNotValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, string message)
        {
            return builder.IsNotValidWhen(expression, _ => message, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsNotValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, DuckValidationRuleOptions options)
        {
            return builder.IsNotValidWhen(expression, _ => null, options);
        }

        public static DuckValidatorBuilder<T> IsNotValidWhen<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.IsNotValidWhen(expression, _ => null, DuckValidationRuleOptions.None);
        }

        public static DuckValidatorBuilder<T> IsNotValidWhenNull<T>(this DuckValidatorBuilder<T> builder)
        {
            return builder.IsNotValidWhen(CreateIsNullLambdaExpression<T>(), _ => null, DuckValidationRuleOptions.BreakOnFailure);
        }

        //public static DuckValidatorBuilder<T> IsNotValidWhenNull<T>(this DuckValidatorBuilder<T> builder, Expression<Func<T, bool>> expression, DuckValidationRuleOptions options = DuckValidationRuleOptions.None)
        //{
        //    return builder.IsNotValidWhen(CreateIsNullLambdaExpression(expression), _ => null, options);
        //}

        #endregion

        private static Expression<Func<T, bool>> CreateIsNullLambdaExpression<T>(Expression<Func<T, bool>> expression)
        {
            return CreateIsNullLambdaExpression<T>(expression.Parameters[0]);
        }

        private static Expression<Func<T, bool>> CreateIsNullLambdaExpression<T>()
        {
            return CreateIsNullLambdaExpression<T>(Expression.Parameter(typeof(T)));
        }

        private static Expression<Func<T, bool>> CreateIsNullLambdaExpression<T>(ParameterExpression parameter)
        {
            //var parameter = Expression.Parameter(typeof(T));
            var equalsNullExpression =
                Expression.Lambda<Func<T, bool>>(
                    Expression.ReferenceEqual(
                        parameter,
                        Expression.Constant(default(T))),
                    parameter
                );

            return equalsNullExpression;
        }
    }
}