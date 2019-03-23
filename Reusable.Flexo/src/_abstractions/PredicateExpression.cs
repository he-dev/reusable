using JetBrains.Annotations;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class PredicateExpression : Expression<bool>
    {
        protected PredicateExpression(string name) : base(name) { }
    }
}