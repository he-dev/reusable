using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public class Matches : PredicateExpression, IExtension<string>
    {
        public Matches() : base(nameof(Matches)) { }

        public bool IgnoreCase { get; set; } = true;

        public IExpression Value { get; set; }

        public IExpression Pattern { get; set; }        
                
        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var value = (string)ExtensionInputOrDefault(ref context, Value).Invoke(context).Value;
            var pattern = (string)Pattern.Invoke(context).Value;
            
            if (value is null)
            {
                return (Name, false, context);
            }
            else
            {
                var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return (Name, Regex.IsMatch(value, pattern, options), context);
            }
        }

//        private string GetPattern(IExpressionContext context)
//        {
//            switch (Pattern)
//            {
//                case string pattern: return pattern;
//                case IExpression expression: return expression.Invoke(context).Value<string>();
//                default:
//                    throw DynamicException.Create
//                    (
//                        "UnsupportedPattern",
//                        $"{Pattern.GetType().ToPrettyString()} is not supported. Expected '{nameof(String)}' or '{nameof(IExpression)}'."
//                    );
//            }
//        }
    }
}