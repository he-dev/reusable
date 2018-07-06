using System;
using System.Linq.Custom;
using System.Linq.Expressions;

namespace Reusable.Validation
{
    public static class DuckValidatorExtensions
    {
        #region IsValidWhen overloads

        public static IDuckValidator<T> IsValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DuckValidationRuleOptions options)
        {
            return new DuckValidator<T>(validator.Append(new DuckValidationRule<T>(expression, messageFactory, options)));
        }

        public static IDuckValidator<T> IsValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsValidWhen(expression, messageFactory, DuckValidationRuleOptions.None);
        }

        public static IDuckValidator<T> IsValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsValidWhen(expression, _ => message, DuckValidationRuleOptions.None);
        }

        public static IDuckValidator<T> IsValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, DuckValidationRuleOptions options)
        {
            return validator.IsValidWhen(expression, _ => null, options);
        }

        public static IDuckValidator<T> IsValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsValidWhen(expression, _ => null, DuckValidationRuleOptions.None);
        }

        #endregion

        #region IsNotValidWhen overloads

        public static IDuckValidator<T> IsNotValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DuckValidationRuleOptions options)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return validator.IsValidWhen(notExpression, messageFactory, options);
        }

        public static IDuckValidator<T> IsNotValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsNotValidWhen(expression, messageFactory, DuckValidationRuleOptions.None);
        }

        public static IDuckValidator<T> IsNotValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsNotValidWhen(expression, _ => message, DuckValidationRuleOptions.None);
        }

        public static IDuckValidator<T> IsNotValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression, DuckValidationRuleOptions options)
        {
            return validator.IsNotValidWhen(expression, _ => null, options);
        }

        public static IDuckValidator<T> IsNotValidWhen<T>(this IDuckValidator<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsNotValidWhen(expression, _ => null, DuckValidationRuleOptions.None);
        }

        #endregion
    }
}