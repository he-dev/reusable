using System;
using System.Linq.Expressions;

namespace Reusable
{
    public static class ExpressionFactory
    {
        public static Expression<Func<T, bool>> ToExpression<T>(this Func<T, bool> func)
        {
            return x => func(x);
        }

        public static Expression<Func<T, bool>> Create<T>(Func<T, bool> func)
        {
            return (Expression<Func<T, bool>>)(x => func(x));
        }
    }
}
