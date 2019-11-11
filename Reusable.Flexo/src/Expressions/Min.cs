using System.Linq;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Min : Aggregate
    {
        public Min() : base(default, Enumerable.Min) { }
    }
}