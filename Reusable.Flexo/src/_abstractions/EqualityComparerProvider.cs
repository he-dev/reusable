using System.Collections.Generic;

namespace Reusable.Flexo
{
    public abstract class EqualityComparerProvider : Expression<IEqualityComparer<object>>
    {
        protected EqualityComparerProvider(string name) : base(name) { }
    }
}