using System.Linq;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Max : Aggregate
    {
        public Max() : base(default, Enumerable.Max) { }
    }
}