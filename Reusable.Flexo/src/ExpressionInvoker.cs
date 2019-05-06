using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    public interface IExpressionInvoker
    {
        IList<(IConstant Result, IImmutableSession Context)> Invoke(IEnumerable<IExpression> expressions, Func<IImmutableSession, IImmutableSession> customizeContext);

        (IConstant Result, IImmutableSession Context) Invoke(IExpression expression, Func<IImmutableSession, IImmutableSession> customizeContext = default);
    }

    public class ExpressionInvoker : IExpressionInvoker
    {
        public IList<(IConstant Result, IImmutableSession Context)> Invoke
        (
            IEnumerable<IExpression> expressions,
            Func<IImmutableSession, IImmutableSession> customizeContext = default
        )
        {
            return expressions.Enabled().Select(e =>
            {
                using (Expression.BeginScope(customizeContext ?? (_ => _)))
                {
                    try
                    {
                        return (e.Invoke(), Expression.Scope.Context);
                    }
                    catch (Exception inner)
                    {
                        return (Constant.Create("Error", inner), Expression.Scope.Context);
                    }
                }
            }).ToList();
        }

        public (IConstant Result, IImmutableSession Context) Invoke
        (
            IExpression expression,
            Func<IImmutableSession, IImmutableSession> customizeContext = default
        )
        {
            return Invoke(new[] { expression }, customizeContext).Single();
        }
    }
}