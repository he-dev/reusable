using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Count : Aggregate
    {
        // The aggregate works with doubles and it does need it for counting too.
        public Count() : base(default, items => items.Count()) { }        
    }
}