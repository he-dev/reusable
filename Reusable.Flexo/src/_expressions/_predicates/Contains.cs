using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public Contains() : base(nameof(Contains)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var value = Value.Invoke(context).Value;
            var comparer = (IEqualityComparer<object>)Comparer?.Invoke(context).Value ?? EqualityComparer<object>.Default;

            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                return (Name, input.Any(x => comparer.Equals(value, x)), context);
            }
            else
            {
                var values = Values.Enabled().Select(x => x.Invoke(context).Value);
                return (Name, values.Any(x => comparer.Equals(value, x)), context);
            }
        }
    }
}