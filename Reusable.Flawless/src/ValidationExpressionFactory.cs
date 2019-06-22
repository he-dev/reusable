using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    using static ValidationExpressionFactory;

    internal static class ValidationExpressionFactory
    {
        public static LambdaExpression ReferenceEqualNull<T>()
        {
            return ReferenceEqualNull<T>(Expression.Parameter(typeof(T)));
        }

        public static LambdaExpression ReferenceEqualNull<T>(Expression<Func<T>> expression)
        {
            // x => object.ReferenceEqual(x.Member, null)

            var member = ParameterSearch.FindParameter(expression);
            var parameter = Expression.Parameter(member.Type);
            var expressionWithParameter = ValidationParameterInjector.InjectParameter(expression.Body, parameter);
            return ReferenceEqualNull<T>(parameter, expressionWithParameter);

            var equalNullExpression =
                Expression.Lambda<Func<bool>>(
                    Expression.ReferenceEqual(
                        expression.Body,
                        Expression.Constant(default(T))
                    ),
                    expression.Parameters[0]
                );

            return equalNullExpression;
        }

        private static LambdaExpression ReferenceEqualNull<T>(ParameterExpression parameter, Expression value = default)
        {
            // x => object.ReferenceEqual(x, null)


            var refEq = Expression.ReferenceEqual(
                value ?? parameter,
                Expression.Constant(default(T)));
                
            return
                Expression.Lambda(
                    Expression.ReferenceEqual(
                        value ?? parameter,
                        Expression.Constant(default(T))),
                    parameter
                );
        }

        public static LambdaExpression Negate<T>(LambdaExpression expression)
        {
            // !x
            return Expression.Lambda(Expression.Not(expression.Body), expression.Parameters[0]);
        }
    }

    internal class ParameterSearch : ExpressionVisitor
    {
        private MemberExpression _parameter;

        public static MemberExpression FindParameter(Expression expression)
        {
            var parameterSearch = new ParameterSearch();
            parameterSearch.Visit(expression);
            return parameterSearch._parameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var isClosure =
                node.Expression.Type.Name.StartsWith("<>c__DisplayClass") &&
                node.Expression.Type.IsDefined(typeof(CompilerGeneratedAttribute));

            if (isClosure)
            {
                _parameter = node;
            }

            return base.VisitMember(node);
        }

//        protected override Expression VisitConstant(ConstantExpression node)
//        {
//            var isClosure =
//                node.Type.Name.StartsWith("<>c__DisplayClass") &&
//                node.Type.IsDefined(typeof(CompilerGeneratedAttribute));
//
//            if (isClosure)
//            {
//                _constant = node;
//            }
//
//            return base.VisitConstant(node);
//        }
    }
}