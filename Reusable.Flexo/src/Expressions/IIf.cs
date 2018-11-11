using System;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Expression
    {
        public IIf() : base(nameof(IIf)) { }

        public IExpression Predicate { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                var expression =
                    (Predicate.SafeInvoke(context).Log().Value<bool>() ? True : False)
                        ?? throw new InvalidOperationException($"{nameof(True)} or {nameof(False)} expression is not defined."); ;

                return expression.SafeInvoke(context).Log();
            }
        }
    }
}