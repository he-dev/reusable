using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Collection : Expression
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name ?? nameof(Collection), ExpressionContext.Empty) { }

        public List<IExpression> Values { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            //var values = Values.Select(Constant.FromValueOrDefault("Item"));
            return Constant.FromValue(Name, Values);
        }
    }

    public interface ICollectionContext
    {
        object Item { get; }
    }
}