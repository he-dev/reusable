using System.Linq;

namespace Reusable.Collections.Generators
{
    public class ConstantSequence<T> : Sequence<T>
    {
        public ConstantSequence(T value) : base(Sequence.Infinite<T>().Select(_ => value)) { }
    }
}