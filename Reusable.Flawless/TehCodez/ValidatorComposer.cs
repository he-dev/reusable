using System;
using System.Linq.Expressions;

namespace Reusable.Flawless
{
    public static class ValidatorComposer
    {
        public static Validator<T> IsValidWhen<T>(this Validator<T> validator, Expression<Func<T, bool>> expression, ValidationOptions options = ValidationOptions.None)
        {
            return validator + new ValidationRule<T>(expression, options);
        }

        public static Validator<T> IsNotValidWhen<T>(this Validator<T> validator, Expression<Func<T, bool>> expression, ValidationOptions options = ValidationOptions.None)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return validator.IsValidWhen(notExpression, options);
        }
    }
}