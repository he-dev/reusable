using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Collection : Expression<List<IExpression>>
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name ?? nameof(Collection), ExpressionContext.Empty) { }

        public List<IExpression> Values { get; set; }

        protected override ExpressionResult<List<IExpression>> InvokeCore(IExpressionContext context)
        {
            return (Values, context);
        }
    }

    public interface ICollectionContext
    {
        object Item { get; }
    }
}