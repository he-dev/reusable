using System;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;

namespace Reusable.Flawless
{
    public static class ValidatorComposer
    {
        #region IsValidWhen overloads

        public static IValidator<T> IsValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, ValidationOptions options)
        {
            return new Validator<T>(validator.Append(new ValidationRule<T>(expression, messageFactory, options)));
        }

        public static IValidator<T> IsValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsValidWhen(expression, messageFactory, ValidationOptions.None);
        }

        public static IValidator<T> IsValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsValidWhen(expression, _ => message, ValidationOptions.None);
        }

        public static IValidator<T> IsValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, ValidationOptions options)
        {
            return validator.IsValidWhen(expression, _ => null, options);
        }

        public static IValidator<T> IsValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsValidWhen(expression, _ => null, ValidationOptions.None);
        }

        #endregion

        #region IsNotValidWhen overloads

        public static IValidator<T> IsNotValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, ValidationOptions options)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return validator.IsValidWhen(notExpression, messageFactory, options);
        }

        public static IValidator<T> IsNotValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsNotValidWhen(expression, messageFactory, ValidationOptions.None);
        }

        public static IValidator<T> IsNotValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsNotValidWhen(expression, _ => message, ValidationOptions.None);
        }

        public static IValidator<T> IsNotValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression, ValidationOptions options)
        {
            return validator.IsNotValidWhen(expression, _ => null, options);
        }

        public static IValidator<T> IsNotValidWhen<T>(this IValidator<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsNotValidWhen(expression, _ => null, ValidationOptions.None);
        }

        #endregion
    }
}