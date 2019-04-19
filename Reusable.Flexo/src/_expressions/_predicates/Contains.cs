using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public Contains(ILogger<Contains> logger) : base(logger, nameof(Contains)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Value { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var value = Value.Invoke(context).Value;
            var comparer = context.GetComparerOrDefault(Comparer);

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