using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    [Obsolete("Use Invoke(context) extension.")]
    public interface IExpressionInvoker
    {
        IList<(IConstant Result, IImmutableContainer Context)> Invoke(IEnumerable<IExpression> expressions, IImmutableContainer? scope = default);

        ExpressionResult Invoke(IExpression expression, IImmutableContainer? scope = default);
    }

    public class ExpressionInvoker : IExpressionInvoker
    {
        public static IExpressionInvoker Default { get; } = new ExpressionInvoker();

        public IList<(IConstant Result, IImmutableContainer Context)> Invoke(IEnumerable<IExpression> expressions, IImmutableContainer? scope = default)
        {
            return expressions.Enabled().Select(e =>
            {
                scope = ExpressionContext.Default.BeginScope(scope ?? ImmutableContainer.Empty);
                try
                {
                    return (e.Invoke(ExpressionContext.Default, scope), scope);
                }
                catch (Exception inner)
                {
                    return (Constant.FromValue(inner.GetType().Name, inner), scope);
                }
            }).ToList();
        }

        public ExpressionResult Invoke(IExpression expression, IImmutableContainer? scope = default)
        {
            return  Invoke(new[] { expression }, scope).Single();
        }
    }
    
    public readonly struct ExpressionResult
    {
        public ExpressionResult(IConstant constant, IImmutableContainer context)
        {
            Constant = constant;
            Context = context;
        }

        public IConstant Constant { get; }

        public IImmutableContainer Context { get; }
    }
}