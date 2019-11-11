using System.Linq;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Sum : Aggregate
    {
        public Sum() : base(default, Enumerable.Sum) { }
    }
}