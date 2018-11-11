using System;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions.Operators.Binary
{
    public class Equals : PredicateExpression
    {
        public Equals() : base(nameof(Equals)) { }

        public IExpression Expression1 { get; set; }

        public IExpression Expression2 { get; set; }

        protected override bool Calculate(IExpressionContext context)
        {
            var x = Expression1.SafeInvoke(context);
            var y = Expression2.SafeInvoke(context);
            return x.Equals(y);
        }
    }

    public class StartsWith : EqualityExpression
    {
        protected StartsWith() : base(nameof(StartsWith), (x, y) => SoftString.Create(x).StartsWith(y)) { }
    }

    public class EndsWith : EqualityExpression
    {
        protected EndsWith() : base(nameof(EndsWith), (x, y) => SoftString.Create(x).EndsWith(y)) { }
    }

    //public class Matches : EqualityExpression
    //{
    //    protected Matches(string pattern) : base(nameof(Matches), (x, y) => SoftString.Create(x).IsMatch(pattern).Matches(y)) { }
    //}
}