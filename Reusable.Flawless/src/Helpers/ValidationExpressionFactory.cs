using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless.Helpers
{
    internal static class ValidationExpressionFactory
    {
        public static Expression<Func<TValue, bool>> GreaterThan<TValue>(Expression<Func<TValue>> left, TValue right)
        {
            // x => x.Member > value
            return Binary(left, right, Expression.GreaterThan);
        }

        public static Expression<Func<TValue, bool>> Binary<TValue>(Expression<Func<TValue>> left, TValue right, Func<Expression, Expression, Expression> binary)
        {
            // x => x.Member == value
            return
                Expression.Lambda<Func<TValue, bool>>(
                    binary(
                        left.Body,
                        Expression.Constant(right)),
                    Expression.Parameter(typeof(TValue))
                );
        }

        internal static Expression<Func<TValue, bool>> Not<TValue>(Expression<Func<TValue, bool>> expression)
        {
            // !x
            return
                Expression.Lambda<Func<TValue, bool>>(
                    Expression.Not(expression.Body),
                    expression.Parameters
                );
        }
    }
}