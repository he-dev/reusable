using System;
using System.Linq.Custom;
using System.Linq.Expressions;

namespace Reusable.Validation
{
    public static class DataFuseComposer
    {
        #region IsValidWhen overloads

        public static IDataFuse<T> IsValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DataFuseOptions options)
        {
            return new DataFuse<T>(validator.Append(new DataFuseRule<T>(expression, messageFactory, options)));
        }

        public static IDataFuse<T> IsValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsValidWhen(expression, messageFactory, DataFuseOptions.None);
        }

        public static IDataFuse<T> IsValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsValidWhen(expression, _ => message, DataFuseOptions.None);
        }

        public static IDataFuse<T> IsValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, DataFuseOptions options)
        {
            return validator.IsValidWhen(expression, _ => null, options);
        }

        public static IDataFuse<T> IsValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsValidWhen(expression, _ => null, DataFuseOptions.None);
        }

        #endregion

        #region IsNotValidWhen overloads

        public static IDataFuse<T> IsNotValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory, DataFuseOptions options)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return validator.IsValidWhen(notExpression, messageFactory, options);
        }

        public static IDataFuse<T> IsNotValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, Func<T, string> messageFactory)
        {
            return validator.IsNotValidWhen(expression, messageFactory, DataFuseOptions.None);
        }

        public static IDataFuse<T> IsNotValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, string message)
        {
            return validator.IsNotValidWhen(expression, _ => message, DataFuseOptions.None);
        }

        public static IDataFuse<T> IsNotValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression, DataFuseOptions options)
        {
            return validator.IsNotValidWhen(expression, _ => null, options);
        }

        public static IDataFuse<T> IsNotValidWhen<T>(this IDataFuse<T> validator, Expression<Func<T, bool>> expression)
        {
            return validator.IsNotValidWhen(expression, _ => null, DataFuseOptions.None);
        }

        #endregion
    }
}