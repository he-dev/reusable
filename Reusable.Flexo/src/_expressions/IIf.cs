using System;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Expression
    {
        public IIf() : base(nameof(IIf)) { }

        [JsonRequired]
        public IExpression Predicate { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                var expression =
                    (Predicate.InvokeWithValidation(context).Value<bool>() ? True : False)
                        ?? throw new InvalidOperationException($"{nameof(True)} or {nameof(False)} expression is not defined."); ;

                return expression.InvokeWithValidation(context);
            }
        }
    }
}