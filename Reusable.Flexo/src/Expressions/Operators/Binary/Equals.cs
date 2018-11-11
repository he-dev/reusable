using System;
using System.Text.RegularExpressions;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions.Operators.Binary
{
    public class Equals : EqualityExpression
    {
        public Equals() : base(nameof(Equals), (x, y) => SoftString.Comparer.Equals(x, y)) { }
    }

    public class StartsWith : EqualityExpression
    {
        protected StartsWith() : base(nameof(StartsWith), (x, y) => SoftString.Create(x).StartsWith(y)) { }
    }

    public class EndsWith : EqualityExpression
    {
        protected EndsWith() : base(nameof(EndsWith), (x, y) => SoftString.Create(x).EndsWith(y)) { }
    }

    public class Matches : EqualityExpression
    {
        protected Matches() : base(nameof(Matches), (x, pattern) => Regex.IsMatch(x, pattern, RegexOptions.IgnoreCase)) { }
    }
}