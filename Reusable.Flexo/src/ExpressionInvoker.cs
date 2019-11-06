using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    [Obsolete("Use Invoke(context) extension.")]
    public interface IExpressionInvoker
    {
        IList<(IConstant Result, IImmutableContainer Context)> Invoke(IEnumerable<IExpression> expressions, IImmutableContainer scope = default);

        (IConstant Result, IImmutableContainer Context) Invoke(IExpression expression, IImmutableContainer scope = default);
    }

    public class ExpressionInvoker : IExpressionInvoker
    {
        public static IExpressionInvoker Default { get; } = new ExpressionInvoker();

        public IList<(IConstant Result, IImmutableContainer Context)> Invoke
        (
            IEnumerable<IExpression> expressions,
            IImmutableContainer scope = default
        )
        {
            return expressions.Enabled().Select(e =>
            {
                using (Expression.BeginScope(customizeContext ?? (_ => _)))
                {
                    try
                    {
                        scope = ExpressionContext.Default.BeginScope(scope ?? ImmutableContainer.Empty);
                        return (e.Invoke(ExpressionContext.Default, scope), scope);
                    }
                    catch (Exception inner)
                    {
                        return (Constant.FromValue(inner.GetType().Name, inner), Expression.Scope.Context);
                    }
                }
            }).ToList();
        }

        public (IConstant Result, IImmutableContainer Context) Invoke
        (
            IExpression expression,
            Func<IImmutableContainer, IImmutableContainer> customizeContext = default
        )
        {
            return Invoke(new[] { expression }, customizeContext).Single();
        }
    }
}