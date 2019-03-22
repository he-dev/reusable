using System;

namespace Reusable.Flexo
{
    public class LambdaExpression<TResult> : Expression<TResult>
    {
        private readonly Func<IExpressionContext, ExpressionResult<TResult>> _invoke;

        public LambdaExpression(string name, IExpressionContext context, Func<IExpressionContext, ExpressionResult<TResult>> invoke) : base(name, context)
        {
            _invoke = invoke;
        }

        protected override ExpressionResult<TResult> InvokeCore(IExpressionContext context)
        {
            return _invoke(context);
        }        
    }
    
    public static class LambdaExpression
    {

        public static LambdaExpression<bool> Predicate(Func<IExpressionContext, ExpressionResult<bool>> predicate)
        {
            return new LambdaExpression<bool>(nameof(Predicate), ExpressionContext.Empty, predicate);
        }

        public static LambdaExpression<double> Double(Func<IExpressionContext, ExpressionResult<double>> calculate)
        {
            return new LambdaExpression<double>(nameof(Double), ExpressionContext.Empty, calculate);
        }
    }
}