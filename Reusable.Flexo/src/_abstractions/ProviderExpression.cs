using System.Collections.Generic;

namespace Reusable.Flexo
{
    public abstract class ProviderExpression<T> : Expression<T>
    {
        protected ProviderExpression(string name) : base(name) { }
    }
}