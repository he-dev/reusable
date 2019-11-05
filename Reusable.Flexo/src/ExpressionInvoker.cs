using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    [Obsolete("Use Invoke(context) extension.")]
    public interface IExpressionInvoker
    {
        IList<(IConstant Result, IImmutableContainer Context)> Invoke(IEnumerable<IExpression> expressions, Func<IImmutableContainer, IImmutableContainer> customizeContext);

        (IConstant Result, IImmutableContainer Context) Invoke(IExpression expression, Func<IImmutableContainer, IImmutableContainer> customizeContext = default);
    }

    public class ExpressionInvoker : IExpressionInvoker
    {
        public static IExpressionInvoker Default { get; } = new ExpressionInvoker();

        public IList<(IConstant Result, IImmutableContainer Context)> Invoke
        (
            IEnumerable<IExpression> expressions,
            Func<IImmutableContainer, IImmutableContainer> customizeContext = default
        )
        {
            return expressions.Enabled().Select(e =>
            {
                using (Expression.BeginScope(customizeContext ?? (_ => _)))
                {
                    try
                    {
                        return (e.Invoke(TODO), Expression.Scope.Context);
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