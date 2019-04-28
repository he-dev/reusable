using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Collection : Expression<List<IConstant>>
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name ?? nameof(Collection)) { }

        public List<IExpression> Values { get; set; }

        protected override Constant<List<IConstant>> InvokeCore(IImmutableSession context)
        {
            return 
            (
                Name,
                Values.Enabled().Select((e, i) => Constant.FromValue($"Item-{i}", e.Invoke(context).Value)).Cast<IConstant>().ToList(),
                context
            );
        }
    }
}