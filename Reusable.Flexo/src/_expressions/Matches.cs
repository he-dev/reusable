using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public class Matches : PredicateExpression, IExtension<string>, IExpressionEqualityComparer
    {
        public Matches() : base(nameof(Matches), ExpressionContext.Empty) { }

        public bool IgnoreCase { get; set; } = true;

        public object Value { get; set; }

        public object Pattern { get; set; }

        #region Comparer
        
        // Lets Matches act as a comparer by mapping Left & Right to the respective own properties.

        IExpression IExpressionEqualityComparer.Left
        {
            get => Constant.FromValueOrDefault(nameof(Pattern), Pattern);
            set => Pattern = value;
        }

        IExpression IExpressionEqualityComparer.Right
        {
            get => Constant.FromValueOrDefault(nameof(Value), Value);
            set => Value = value;
        }

        #endregion

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var value = (string)Constant.FromValueOrDefault(Name, Value).Invoke(context).ValueOrDefault();
            if (value is null)
            {
                return (false, context);
            }
            else
            {
                var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return (Regex.IsMatch(value, GetPattern(context), options), context);
            }
        }

        private string GetPattern(IExpressionContext context)
        {
            switch (Pattern)
            {
                case string pattern: return pattern;
                case IExpression expression: return (string)expression.Invoke(context).Value();
                default:
                    throw DynamicException.Create
                    (
                        "UnsupportedPattern",
                        $"{Pattern.GetType().ToPrettyString()} is not supported. Expected '{nameof(String)}' or '{nameof(IExpression)}'."
                    );
            }
        }
    }
}